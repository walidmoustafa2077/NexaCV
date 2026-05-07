import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { getResumes, deleteResume, renameResume } from "@/lib/api/resumes";
import { queryKeys } from "@/lib/query/keys";

export function useResumes() {
    return useQuery({
        queryKey: queryKeys.resumes(),
        queryFn: getResumes,
    });
}

export function useDeleteResume() {
    const qc = useQueryClient();
    return useMutation({
        mutationFn: deleteResume,
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: queryKeys.resumes() });
        },
    });
}

export function useRenameResume() {
    const qc = useQueryClient();
    return useMutation({
        mutationFn: ({ id, name }: { id: string; name: string }) => renameResume(id, name),
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: queryKeys.resumes() });
        },
    });
}
