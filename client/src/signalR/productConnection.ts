import { getConnection, leave, start } from "./useConnection";
import * as signalR from "@microsoft/signalr";

let connection: signalR.HubConnection | null = null;

export function getProductConnection(): signalR.HubConnection  {
    return getConnection(connection, "product");
}

export async function startProductConnection(connection: signalR.HubConnection, productId?: string | null) {
    return start(connection, "JoinProductGroup", productId);
}

export async function leaveProductGroup(connection: signalR.HubConnection, productId?: string | null) {
    return leave(connection, "LeaveProductGroup", productId);
}