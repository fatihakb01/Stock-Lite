import { useEffect } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { getCompanyConnection, leaveCompanyGroup, startCompanyConnection } from "../signalR/companyConnection";
import { useCurrentUser } from "./useUsers";

export function useCompanyRealtime() {
    const queryClient = useQueryClient();
    const { data: currentUser } = useCurrentUser();

    useEffect(() => {
        if (!currentUser?.companyId) return;

        const connection = getCompanyConnection();

        connection.on("ProductsUpdated", (companyId: string) => {
            if (currentUser?.companyId === companyId) {
                queryClient.invalidateQueries({ queryKey: ["products", companyId] });
                queryClient.invalidateQueries({ queryKey: ["products", companyId, "BelowSafetyStock"] });
                queryClient.invalidateQueries({ queryKey: ["products", companyId, "WithRecommendedOrders"] });
                queryClient.invalidateQueries({ queryKey: ["products", companyId, "WithoutStockAtLeadTime"] });
                queryClient.invalidateQueries({ queryKey: ["products", companyId, "WithoutForecast"] });
            }
        });

        connection.on("ProductKpisUpdated", (companyId: string) => {
            if (currentUser?.companyId === companyId) {
                queryClient.invalidateQueries({ queryKey: ["productkpi", "home", companyId] });
                queryClient.invalidateQueries({ queryKey: ["productkpi", "schedulerStatus", companyId] });
            }
        });

        connection.on("SchedulerStatusUpdated", (companyId: string) => {
            if (currentUser?.companyId === companyId) {
                queryClient.invalidateQueries({ queryKey: ["productkpi", "schedulerStatus", companyId] });
            }
        });

        connection.on("JobTaskUpdated", (jobTaskId: string) => {
            queryClient.invalidateQueries({ queryKey: ["jobtask", jobTaskId] });
            queryClient.invalidateQueries({ queryKey: ["jobtasks", currentUser.companyId] });
            queryClient.invalidateQueries({ queryKey: ["productkpi", "schedulerStatus", currentUser.companyId] });
        });

        connection.on("ScheduledJobUpdated", (scheduledJobId: string, jobTaskId: string) => {
            queryClient.invalidateQueries({ queryKey: ["jobtask", jobTaskId] });
            queryClient.invalidateQueries({ queryKey: ["jobtasks", currentUser.companyId] });
            queryClient.invalidateQueries({ queryKey: ["scheduledjob", scheduledJobId] });
            queryClient.invalidateQueries({ queryKey: ["productkpi", "schedulerStatus", currentUser.companyId] });
        });

        startCompanyConnection(connection, currentUser.companyId);

        return () => {
            connection.off("ProductsUpdated");
            connection.off("ProductKpisUpdated");
            connection.off("SchedulerStatusUpdated");
            connection.off("JobTaskUpdated");
            connection.off("ScheduledJobUpdated");
            leaveCompanyGroup(connection, currentUser.companyId);
        };
    }, [queryClient, currentUser?.companyId]);
}