import * as signalR from "@microsoft/signalr";

export function getConnection(connection: signalR.HubConnection | null, name: string)
    : signalR.HubConnection {
    if (!connection) {
        connection = new signalR.HubConnectionBuilder()
            .withUrl(`${import.meta.env.VITE_URL}/hubs/${name}`, {
                accessTokenFactory: () => localStorage.getItem("token") || ""
            })
            .withAutomaticReconnect()
            .build();
    }
    return connection;
}

export async function start(connection: signalR.HubConnection, methodName: string, id?: string | null) {
    try {
        if (id && connection.state === signalR.HubConnectionState.Disconnected) {
            await connection.start();
            await connection.invoke(methodName, id);
        }
    } catch (error) {
        console.error("Failed to start connection:", error);
    }
}

export async function leave(connection: signalR.HubConnection, methodName: string, id?: string | null) {
    try {
        if (id && connection.state === signalR.HubConnectionState.Connected) {
            await connection.invoke(methodName, id);
        }
    } catch (error) {
        console.error("Failed to leave group:", error);
    }
}