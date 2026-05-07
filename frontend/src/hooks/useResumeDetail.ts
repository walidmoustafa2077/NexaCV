import { useQuery } from "@tanstack/react-query";
import { getResume } from "@/lib/api/resumes";
import { queryKeys } from "@/lib/query/keys";

export function useResumeDetail(id: string) {
    return useQuery({
        queryKey: queryKeys.resume(id),
        queryFn: () => getResume(id),
        enabled: !!id,
        staleTime: 1000 * 60 * 5,
        retry: 1,
    });
}
