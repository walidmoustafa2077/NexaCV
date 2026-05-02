# Diagram: AI Regeneration Sequence
**Location:** `Documentations/resources/flow-diagrams/04-AI-Regeneration-Sequence.md`  
**Scope:** Full interaction when a user clicks "Regenerate" on a specific section, including the 3-attempt limit enforcement (SRD §3.2.3.3, §3.2.3.4).

---

## Sequence Diagram

![AI Regeneration Sequence](../../images/AI%20Regeneration%20Sequence.png)

---

## Regeneration Limit Enforcement Logic

![AI Regeneration Sub-Flow](../../images/AI%20Regeneration%20Sub-Flow.png)

---

## Cost Accumulation Model

| Event | EGP Added | USD Added |
| :--- | :--- | :--- |
| 1st regeneration on any section | +10.00 | +0.25 |
| 2nd regeneration on same section | +10.00 | +0.25 |
| 3rd (final) regeneration on same section | +10.00 | +0.25 |
| 4th attempt on same section | **Blocked** (429) | — |
| Total max per section | **30.00 EGP** | **0.75 USD** |

The final total billed at checkout = `templates.base_price` + `SUM(regenerations.cost)` for all sections of that resume.
