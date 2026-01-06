export const poStatusMap: Record<number, string> = {
  0: "Draft",
  1: "Submitted",
  2: "PartiallyReceived",
  3: "Completed",
  4: "Cancelled"
};

export const reversePoStatusMap: Record<string, number> = {
  Draft: 0,
  Submitted: 1,
  PartiallyReceived: 2,
  Completed: 3,
  Cancelled: 4
};
