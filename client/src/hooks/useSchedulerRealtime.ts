// hooks/useSchedulerRealtime.ts
import { useEffect } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { getSchedulerConnection } from "../signalR/schedulerConnection";
import * as signalR from "@microsoft/signalr";

export function useSchedulerRealtime() {
    const queryClient = useQueryClient();

    useEffect(() => {
        const connection = getSchedulerConnection();

        connection.on("JobTaskUpdated", () => {
            queryClient.invalidateQueries({ queryKey: ["jobtask"] });
            queryClient.invalidateQueries({ queryKey: ["scheduledjob"] });
        });

        connection.on("ScheduledJobUpdated", () => {
            queryClient.invalidateQueries({ queryKey: ["jobtask"] });
            queryClient.invalidateQueries({ queryKey: ["jobtasks"] });
            queryClient.invalidateQueries({ queryKey: ["scheduledjob"] });
        });

        const start = async () => {
            if (connection.state === signalR.HubConnectionState.Disconnected) {
                await connection.start();
                await connection.invoke("JoinCompanyGroup");
            }
        };

        start();

        return () => {
            connection.off("JobTaskUpdated");
            connection.off("ScheduledJobUpdated");
        };
    }, [queryClient]);
}