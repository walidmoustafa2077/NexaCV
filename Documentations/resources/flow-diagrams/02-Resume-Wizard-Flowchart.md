# Diagram: Resume Multi-Step Wizard Flowchart
**Location:** `Documentations/resources/flow-diagrams/02-Resume-Wizard-Flowchart.md`  
**Scope:** Step-by-step user journey through the data-entry wizard (SRD §3.2.2, §3.12.2).

---

![State Machine Diagram](../../images/State%20Machine%20Diagram.png)

---

## Wizard Steps Summary

| Step | Section | Required Fields | Optional Fields |
| :--- | :--- | :--- | :--- |
| 1 | Personal Info | Full Name, Email, Phone, Location | LinkedIn, Portfolio URL |
| 2 | Work Experience | Job Title, Company | Dates, Bullet Points (repeatable) |
| 3 | Education | — | Degree, Institution, Dates (repeatable) |
| 4 | Optional Extras | — | Skills, Projects, Certs, Languages, Awards, Volunteer, Publications, Interests, Summary |
| 5 | Review & Submit | Confirm all entries | — |

> **UX Note (Req 3.12.2):** Each step is shown as a separate page/screen — not as a long scrolling form — to reduce cognitive load.
