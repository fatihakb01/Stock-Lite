export const coStatusMap: Record<number, string> = {
  0: "Confirmed",
  1: "PartiallyFulfilled",
  2: "Fulfilled",
  3: "Cancelled",
};

export const reverseCoStatusMap: Record<string, number> = {
  Confirmed: 0,
  PartiallyFulfilled: 1,
  Fulfilled: 2,
  Cancelled: 3,
};
