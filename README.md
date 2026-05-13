# Smart Email Assistant - Semantic Kernel POC

> AI-powered email and calendar orchestration in .NET, without losing control.

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12.0-239120?style=flat-square&logo=csharp)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![Semantic Kernel](https://img.shields.io/badge/Semantic%20Kernel-1.x-yellow?style=flat-square&logo=microsoft)](https://github.com/microsoft/semantic-kernel)
[![OpenAI](https://img.shields.io/badge/OpenAI-GPT--4o--mini-412991?style=flat-square&logo=openai)](https://openai.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue?style=flat-square)](LICENSE)

---

## Read the Full Architecture Breakdown

Companion code for my Medium article:

**[How to Add AI to Your .NET App Without Losing Control](https://medium.com/@krupali.vithlani/how-to-add-ai-to-your-net-app-without-losing-control-8e99f8103ac1)**

It covers the *why* behind every design decision: Semantic Kernel internals, orchestration vs autonomy, and what production-safe AI looks like in a .NET codebase.

---

## The Problem

**Tuesday, 9:47 AM.** You are finally in flow state.

_Ding._

```
From: Priya Mehta, Product Manager
Subject: Quick release sync?

Hey! Can we grab 30 minutes Tuesday at 2 PM to discuss the Q1 release?
```

Now begins the coordination tax:

1. Open calendar - notice a conflict at 2 PM
2. Context switch - which Q1 release? API rewrite or mobile launch?
3. Find alternatives - scan your week for free slots
4. Draft a response - type something professional
5. Rewrite it - make it sound friendly but not too casual
6. Send it - finally
7. Recover focus - 10 minutes to get back into flow

**Total time lost: 25 minutes. Multiply by 3-4 emails a week. You are losing weeks every year.**

---

## The Solution - Orchestration, Not Autonomy

You do not need an autonomous agent that books meetings and sends emails on your behalf. That is a liability in production.

You need a system that:

- Understands natural language like "Tuesday at 2 PM"
- Checks your calendar against your actual availability
- Knows which projects are active and their priority
- Drafts a professional response in your communication style
- Suggests options - but **you** make the final call

**This is not autonomy. This is orchestration.**

---

## How It Works

```
Email arrives
    |
    v
AI: Extract meeting details
    "Tuesday at 2 PM, Q1 release discussion"
    |
    v
CalendarPlugin: Check availability
    "Conflict - Engineering standup at 2 PM"
    |
    v
CalendarPlugin: Find alternative slots
    "Free at 3 PM or Wednesday 10 AM"
    |
    v
AI: Generate professional draft response
    "Hi Priya, I'm booked at 2 PM. How about 3 PM or Wed 10 AM?"
    |
    v
YOU: Review and approve
    One decision. Not twenty-five minutes.
```

---

## Project Structure

```
SmartEmailAssistant/
|
+-- Plugins/
|   +-- CalendarPlugin.cs    # Checks availability, finds conflicts, suggests alternatives
|   +-- EmailPlugin.cs       # Drafts, formats, and manages email responses
|
+-- Services/                # Service abstractions over external integrations
|
+-- Models/                  # Data models - email metadata, calendar slots, etc.
|
+-- Program.cs               # Kernel setup, plugin registration, interactive chat loop
+-- SmartEmailAssistant.csproj
+-- SmartEmailAssistant.sln
```

---

## Key Technical Decisions

### 1. Semantic Kernel as the Orchestrator

Microsoft's Semantic Kernel acts as the conductor. It does not replace your business logic, it coordinates it. The Kernel manages connections between the AI model, your plugins, and chat history, while your application keeps full control over what runs and when.

### 2. Custom Plugins - Wrapping Business Logic as AI-Callable Functions

`CalendarPlugin` and `EmailPlugin` are plain C# classes decorated with `[KernelFunction]` attributes. The AI invokes the right function based on natural language intent. No hardcoded routing or intent classifiers.

```csharp
builder.Plugins.AddFromType<CalendarPlugin>();
builder.Plugins.AddFromType<EmailPlugin>();
```

### 3. AutoInvokeKernelFunctions - Automatic Tool Dispatch

The model decides which plugin to call and when, based on conversation context. Clean, declarative, and no switch statements needed.

```csharp
executionSettings: new OpenAIPromptExecutionSettings()
{
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
}
```

### 4. Streaming Responses

Uses `GetStreamingChatMessageContentsAsync` so responses stream token-by-token. The assistant feels immediate rather than waiting on a slow API round-trip.

```csharp
await foreach (var messagePart in completion)
{
    Console.Write(messagePart.Content);
    fullMessage += messagePart.Content;
}
```

### 5. ChatHistory with System Persona

A persistent `ChatHistory` with a system-level persona maintains full context across multi-turn conversations. The assistant remembers earlier messages, project context, and preferences throughout the session.

```csharp
chatMessages.AddSystemMessage(@"
    You are a helpful assistant that manages emails and calendar.
    When responding to meeting requests:
    1. Always check calendar availability first
    2. Check if the topic relates to any active projects
    3. Draft appropriate email responses
    4. Suggest alternative times if the user is busy
    Be concise and professional.");
```

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- An [OpenAI API key](https://platform.openai.com/api-keys) - the project uses `gpt-4o-mini`

---

## Getting Started

**1. Clone the repository**
```bash
git clone https://github.com/vithlani/semantic-kernel-smart-assistant.git
cd semantic-kernel-smart-assistant
```

**2. Create a `.env` file** in the project root
```
OPENAI_API_KEY=sk-your-key-here
```

**3. Restore dependencies**
```bash
dotnet restore
```

**4. Run**
```bash
dotnet run
```

---

## Sample Interaction

```
You: Priya wants to meet Tuesday at 2 PM for Q1 release sync

Assistant: Checked your calendar - you have Engineering standup at 2 PM on Tuesday.

           Available alternatives:
           - Tuesday at 3:00 PM
           - Wednesday at 10:00 AM
           - Wednesday at 2:00 PM

           Draft response:
           ------------------------------------------------
           Hi Priya, thanks for reaching out! I would love
           to sync on the Q1 release, but I have a conflict
           at 2 PM on Tuesday. Would Tuesday 3 PM or
           Wednesday 10 AM work for you?
           ------------------------------------------------

           Send this, or ask me to adjust the tone.

You: Make it more casual

Assistant: Updated draft:
           ------------------------------------------------
           Hey Priya! Happy to sync on Q1 - I'm actually
           tied up at 2 on Tuesday though. Does 3 PM work,
           or Wednesday morning around 10?
           ------------------------------------------------
```

---

## NuGet Packages Used

| Package | Purpose |
|---|---|
| `Microsoft.SemanticKernel` | Core orchestration framework |
| `Microsoft.SemanticKernel.Connectors.OpenAI` | OpenAI GPT-4o-mini integration |
| `dotenv.net` | Load API key from `.env` file |

---

## What is Next
 
Part 2 of the article series will cover:
 
- Connecting plugins to real calendar and email APIs (Google Calendar, Outlook)
- Adding Memory with vector embeddings for long-term context
- Moving from a console app to an ASP.NET Core web API
- Exploring Azure OpenAI as a drop-in replacement for the OpenAI connector
Follow me on Medium to get notified when it drops.
 
---
 
## Author
 
**Krupali Vithlani** - Senior Software Engineer
 
- Medium: [@krupali.vithlani](https://medium.com/@krupali.vithlani)
- GitHub: [@vithlani](https://github.com/vithlani)
- LinkedIn: [linkedin.com/in/krupali-vithlani](https://linkedin.com/in/krupali-vithlani)
