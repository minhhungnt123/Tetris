# Universal AI Agent Skill Specification

Tài liệu này định nghĩa cấu trúc Skill dùng chung cho các AI Agent như Claude, Codex, Cursor, Gemini, Copilot, Antigravity, OpenHands, Windsurf và các hệ thống tương tự.

---

## 1. Metadata (Bắt buộc)

### YAML Frontmatter

```yaml
---
name: skill-name
description: >
  Mô tả ngắn về mục đích của skill.
version: 1.0.0
domain: software_engineering
author: Your Team
updated: 2026-06-05
---
```

### XML

```xml
<metadata>
  <name>skill-name</name>
  <version>1.0.0</version>
  <description>Mô tả ngắn về skill.</description>
  <domain>software_engineering</domain>
  <author>Your Team</author>
  <updated>2026-06-05</updated>
</metadata>
```

### Quy tắc đặt tên

- Chữ thường.
- Các từ nối bằng dấu gạch ngang (-).
- Tên phải phản ánh đúng chức năng.

Ví dụ:

- api-document-generator
- code-reviewer
- bug-investigator
- database-optimizer

---

## 2. Role (Bắt buộc)

Xác định vai trò mà AI Agent đảm nhận.

### Markdown

```md
## Role

Agent đóng vai trò là [chức danh].

Mục tiêu:
- ...

Trách nhiệm:
- ...

Giới hạn:
- ...
```

### XML

```xml
<role>
  <title>[Chức danh]</title>
  <objective>[Mục tiêu]</objective>
  <responsibilities>
    <item>...</item>
  </responsibilities>
  <limitations>
    <item>...</item>
  </limitations>
</role>
```

---

## 3. Inputs (Bắt buộc)

### Markdown

```md
## Inputs

### Required
- Dữ liệu bắt buộc

### Optional
- Thông tin bổ sung
```

### XML

```xml
<inputs>
  <required>Dữ liệu bắt buộc</required>
  <optional>Thông tin bổ sung</optional>
</inputs>
```

---

## 4. Outputs (Bắt buộc)

### Markdown

```md
## Outputs

Format:
- Markdown
- JSON
- XML
- Source Code

Naming Convention:
<original_name>_output.<ext>

Location:
Same directory as source
```

### XML

```xml
<outputs>
  <format>Markdown</format>
  <naming_convention>
    &lt;original_name&gt;_output.&lt;ext&gt;
  </naming_convention>
  <location>Same directory as source</location>
</outputs>
```

---

## 5. Workflow (Bắt buộc)

Tối đa 7 bước chính.

### Markdown

```md
## Workflow

### Step 1: Validate Input
verify: All required inputs exist.

### Step 2: Analyze
verify: Analysis completed.

### Step 3: Generate Result
verify: Output generated.

### Step 4: Validate Output
verify: Output meets requirements.
```

### XML

```xml
<workflow>
  <step order="1">
    <action>Validate Input</action>
    <verify>Input is valid</verify>
  </step>

  <step order="2">
    <action>Analyze</action>
    <verify>Analysis completed</verify>
  </step>
</workflow>
```
---

## 6. Rules & Constraints (Bắt buộc)

### Markdown

```md
## Rules

### MUST
- Validate inputs.
- Follow workflow.
- Explain assumptions.

### MUST NOT
- Invent data.
- Skip validation.

### Special Cases
- If information is missing -> request clarification.
- If output fails validation -> regenerate.
```
### XML

```xml
<rules>
  <must>
    <rule>Validate inputs</rule>
  </must>

  <must_not>
    <rule>Invent data</rule>
  </must_not>

  <special_cases>
    <case trigger="Missing information"
          action="Request clarification"/>
  </special_cases>
</rules>
```

---

## 7. Examples (Bắt buộc)

### Markdown

```md
## Example

### Input

Create REST API for user management.

### Output

- UserController
- UserService
- UserRepository
- Database Schema
```

### XML

```xml
<examples>
  <example id="1">
    <input>Create REST API for user management</input>
    <output>
      UserController
      UserService
      UserRepository
    </output>
  </example>
</examples>
```

---

## 8. Error Handling (Khuyến khích)

### Markdown

```md
## Error Handling

If required input is missing:
-> Request additional information.

If task is ambiguous:
-> Ask clarifying questions.

If validation fails:
-> Regenerate output.
```

### XML

```xml
<error_handling>
  <case trigger="Missing Input"
        action="Request Information"/>

  <case trigger="Ambiguous Task"
        action="Ask Clarification"/>
</error_handling>
```

---

## 9. Tools (Khuyến khích)

### Markdown

```md
## Tools

Available Tools:
- Web Search
- File System
- Database
- Git
- MCP Server

Usage Rules:
- Use Web Search only when fresh information is required.
- Use Database only when permission is granted.
```
---

## 10. Success Criteria (Khuyến khích)

### Markdown

```md
## Success Criteria

Task is successful when:

- All required outputs are generated.
- Output passes validation.
- No rule violations exist.
- User objectives are satisfied.
```
---

## Agent-Agnostic Design Principles

Skill phải:

1. Không phụ thuộc vào nhà cung cấp AI.
2. Không sử dụng từ ngữ riêng như Claude, GPT, Gemini.
3. Chỉ sử dụng thuật ngữ trung lập:
   - Agent
   - AI Agent
   - Assistant
   - Runtime
   - Tool
   - Workflow
4. Có thể hoạt động trên nhiều nền tảng AI khác nhau.
