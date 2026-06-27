// signalr/schedulerConnection.ts
import * as signalR from "@microsoft/signalr";

let connection: signalR.HubConnection | null = null;

export function getSchedulerConnection(): signalR.HubConnection {
    if (!connection) {
        connection = new signalR.HubConnectionBuilder()
            .withUrl(`${import.meta.env.VITE_URL}/hubs/scheduler`, {
                accessTokenFactory: () => localStorage.getItem("token") || ""
            })
            .withAutomaticReconnect()
            .build();
    }
    return connection;
}