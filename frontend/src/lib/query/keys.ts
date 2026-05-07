export const queryKeys = {
    user: () => ["user"] as const,
    templates: () => ["templates"] as const,
    template: (id: number) => ["templates", id] as const,
    resumes: () => ["resumes"] as const,
    resume: (id: string) => ["resumes", id] as const,
    transaction: (id: string) => ["transactions", id] as const,
} as const;
