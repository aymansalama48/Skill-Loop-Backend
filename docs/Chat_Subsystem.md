# Chat Subsystem — 1-to-1 Real-time Messaging

> **Stack**: Clean Architecture · CQRS (MediatR) · EF Core · SignalR (WebSocket)

## 1. الفكرة
محادثة نصية 1-لـ-1 بين مستخدمين (طالب ↔ معلم). الرسالة بتتحفظ في SQL Server وبتتبعت لحظياً للطرف التاني لو Online.
لو Offline بيشوفها لما يفتح ويعمل `GET messages`.

## 2. الجداول
| Table | أهم الأعمدة | ملاحظات |
|---|---|---|
| `Conversations` | `ParticipantOneId`, `ParticipantTwoId`, `LastMessageAt`, `LastMessagePreview` | **Unique Index** على الزوج. `ParticipantOneId` دايماً الـ Guid الأصغر |
| `ChatMessages` | `ConversationId`, `SenderId`, `Content(≤2000)`, `SentAt`, `ReadAt` | Index على `(ConversationId, SentAt)`. `ReadAt = null` يعني غير مقروءة |

## 3. REST Endpoints (`[Authorize]`) — `api/v1/chat`
| Method | Route | الوظيفة |
|---|---|---|
| POST | `/conversations` | ابدأ (أو رجّع) محادثة. Body: `{ "otherUserId": "..." }` |
| GET | `/conversations` | قايمة محادثاتي + غير المقروء |
| GET | `/conversations/{id}/messages?pageNumber=1&pageSize=20` | الرسايل (الأحدث أول) |
| POST | `/conversations/{id}/messages` | ابعت رسالة. Body: `{ "content": "..." }` |
| POST | `/conversations/{id}/read` | علّم رسايلي كمقروءة |

## 4. SignalR — `wss://<host>/hubs/chat`
التوكن بيتبعت في الـ Query String: `?access_token=<JWT>` (المتصفح مايقدرش يبعت Authorization Header مع WebSocket).

**Client → Server (invoke)**
| Method | Params | Returns |
|---|---|---|
| `SendMessage` | `conversationId`, `content` | `MessageDto` (أو `HubException`) |
| `MarkAsRead` | `conversationId` | — |

**Server → Client (on)**
| Event | Payload |
|---|---|
| `ReceiveMessage` | `MessageDto` `{ id, conversationId, senderId, content, sentAt, readAt }` |
| `MessagesRead` | `{ conversationId, readerId, readAt }` |

## 5. تدفق إرسال رسالة
```
Client ──(REST POST أو Hub.SendMessage)──► SendMessageCommand
   └► Handler: يتأكد إنك طرف في المحادثة → ChatMessage.Create → Save
        └► IChatNotifier.NotifyMessageReceivedAsync ──► SignalR ──► ReceiveMessage عند الطرف التاني
```
> الإشعار اللحظي **مش** بيعدّي على الـ Outbox عمداً (الـ Outbox Job بيشتغل كل 5 ثواني، ودي بطيئة للشات).

## 6. حدود معروفة / تطوير لاحق
- نص فقط (مفيش صور/ملفات). - مفيش حذف/تعديل رسائل. - مفيش "يكتب الآن…".
- لو الطرف Offline: مفيش Push Notification (لما نعمل FCM نضيف Domain Event + Handler).
- Pagination بالـ Offset؛ ممكن يتحول لـ Cursor لو الرسايل كترت.
- شغّل أكتر من نسخة من الـ API؟ هتحتاج Redis backplane لـ SignalR.
- قاعدة "مين يقدر يكلم مين" حالياً: أي مستخدم نشط. لو فيه شرط (مثلاً لازم يكون مشترك في كورس المعلم) بيتضاف في `StartConversationCommandHandler`.
