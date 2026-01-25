export const statusMap: Record<number, string> = {
    0: "Pending",
    1: "Running",
    2: "Succeeded",
    3: "Failed"
};

export const reverseStatusMap: Record<string, number> = {
    Pending: 0,
    Running: 1,
    Succeeded: 2,
    Failed: 3
};
