# Diagram: Entity-Relationship Diagram (ERD)
**Location:** `Documentations/resources/database/ERD-Diagram.md`  
**Scope:** Full database schema for NexaCV — all 7 tables and their relationships.

---

![ERD Diagram](../../images/ERD%20Diagram.png)

---

## Relationship Notes

| Relationship | Cardinality | Description |
| :--- | :--- | :--- |
| `USERS` → `USER_MOVEMENTS` | 1 to Many | Every login, logout, or password update is logged. |
| `USERS` → `RESUMES` | 1 to Many | A single user may build multiple resumes over time. |
| `USERS` → `TRANSACTIONS` | 1 to Many | A user can pay for multiple resumes across sessions. |
| `TEMPLATES` → `RESUMES` | 1 to Many | One template design can be used by many resumes. |
| `RESUMES` → `REGENERATIONS` | 1 to Many | Up to 3 regeneration records per section per resume. |
| `RESUMES` → `TRANSACTIONS` | 1 to 1 | Each resume has exactly one final payment transaction. |
| `RESUMES` → `DOWNLOADS` | 1 to Many | A paid resume can be downloaded multiple times (PDF/DOCX). |

## Key Design Decisions

- **`resumes.raw_data` (JSONB):** Stores the exact user input from the wizard form — arrays of work experience items, education entries, etc. — without requiring separate normalized tables.
- **`resumes.final_data` (JSONB):** Stores the AI-polished version. Sections are individually patchable by the regeneration endpoint.
- **`regenerations.section_identifier`:** Links back to a specific item inside the JSONB blobs (e.g., `WORK_EXP_ID_1`, `SUMMARY`). Each JSON item inside `raw_data` must carry a stable UUID for this linkage.
- **`resumes.status` enum:** `DRAFT` → `COMPLETED` → `PAID` tracks the resume lifecycle.
- **`transactions.payment_status` enum:** `PENDING` → `SUCCESS` / `FAILED`.
