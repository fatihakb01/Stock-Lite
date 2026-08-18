import { getConnection, leave, start } from "./useConnection";
import * as signalR from "@microsoft/signalr";

let connection: signalR.HubConnection | null = null;

export function getCompanyConnection() {
    return getConnection(connection, "company");
}

export async function startCompanyConnection(connection: signalR.HubConnection, companyId: string) {
    return start(connection, "JoinCompanyGroup", companyId);
}

export async function leaveCompanyGroup(connection: signalR.HubConnection, companyId: string) {
    return leave(connection, "LeaveCompanyGroup", companyId);
}