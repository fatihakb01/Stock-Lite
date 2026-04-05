import { useState, useEffect } from "react";

export function useSelection(defaultId?: string) {
    const [selectedId, setSelectedId] = useState<string | null>(defaultId ?? null);

    useEffect(() => {
        if (defaultId && !selectedId)
            setSelectedId(defaultId);
    }, [defaultId]);

    return {
        selectedId,
        select: (id: string) => setSelectedId(id),
        clear: () => setSelectedId(null)
    };
}