# 📚 Skill Loop — Documentation Index

This directory contains detailed documentation for the Skill Loop backend.

## Architecture & Design

| Document | Description |
|----------|-------------|
| [ARCHITECTURE_GUIDE.md](ARCHITECTURE_GUIDE.md) | Layer-by-layer walkthrough — entities, enums, events, behaviors, DI, caching |
| [SECURITY.md](SECURITY.md) | Security controls, secret rotation runbook, configuration requirements, regression tests |
| [PERMISSIONS_MATRIX.md](PERMISSIONS_MATRIX.md) | Role-to-permission reference matrix |

## Subsystem Documentation

| Document | Description |
|----------|-------------|
| [Chat_Subsystem.md](Chat_Subsystem.md) | Real-time chat via SignalR — conversations, messages, read receipts |
| [Course_And_Enrollment_Subsystem.md](Course_And_Enrollment_Subsystem.md) | Courses, sections, lessons, enrollment, and progress tracking |
| [Support_Subsystem.md](Support_Subsystem.md) | FAQ, support requests, staff answering, email notifications, caching |

## Testing & Development

| Document | Description |
|----------|-------------|
| [TESTING_RUNBOOK.md](TESTING_RUNBOOK.md) | How to run and write tests |
| [api-tests.http](api-tests.http) | HTTP request collection for manual API testing (VS Code REST Client / Rider) |
| [skill-loop.postman_collection.json](skill-loop.postman_collection.json) | Postman collection for API testing |
| [chat-test-client.html](chat-test-client.html) | Standalone HTML page for testing SignalR chat |
| [run-api-local.ps1](run-api-local.ps1) | PowerShell script to run the API locally with all required setup |
