export const triggerTypeMap: Record<number, string> = {
    0: "Scheduled",
    1: "Manual",
    2: "Retry"
};

export const reverseTriggerTypeMap: Record<string, number> = {
    Scheduled: 0,
    Manual: 1,
    Retry: 2
};
