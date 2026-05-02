# Diagram: AI Generation Sequence
**Location:** `Documentations/resources/flow-diagrams/03-AI-Generation-Sequence.md`  
**Scope:** Full interaction sequence when the user submits the wizard and OpenAI generates the resume (SRD §3.2.3, §3.11).

---

![AI Generation Sequence](../../images/AI%20Generation%20Sequence.png)

---

## Notes

| Aspect | Detail |
| :--- | :--- |
| **Timeout threshold** | 5 seconds per Req 3.11.1 — if OpenAI does not respond, fallback activates |
| **Prompt security** | API key stored in GCP Secret Manager, never in code or env files committed to source control |
| **JSONB patching** | `final_data` stores each section as a keyed object; downstream regenerations patch individual keys without rewriting the full blob |
| **Autosave** | After every successful AI write, `resumes.updated_at` is set to `NOW()` for data persistence (Req 3.4.3) |
