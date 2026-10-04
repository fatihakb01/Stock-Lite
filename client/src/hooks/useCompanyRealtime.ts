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
                queryClient.invalidateQueries({ queryKey: ["productkpis", companyId] });
                queryClient.invalidateQueries({ queryKey: ["schedulerstatus", companyId] });
            }
        });

        connection.on("SchedulerStatusUpdated", (companyId: string) => {
            if (currentUser?.companyId === companyId) {
                queryClient.invalidateQueries({ queryKey: ["schedulerstatus", companyId] });
            }
        });

        connection.on("JobTasksUpdated", (companyId: string) => {
            if (currentUser?.companyId === companyId) {
                queryClient.invalidateQueries({ queryKey: ["jobtasks", currentUser.companyId] });
            }
        });

        connection.on("JobTaskUpdated", (jobTaskId: string) => {
            queryClient.invalidateQueries({ queryKey: ["jobtask", jobTaskId] });
            queryClient.invalidateQueries({ queryKey: [`jobtask:history:${jobTaskId}`] });
            queryClient.invalidateQueries({ queryKey: ["jobtasks", currentUser.companyId] });
            queryClient.invalidateQueries({ queryKey: [`jobtasks:recent:history:${currentUser.companyId}`] });
            queryClient.invalidateQueries({ queryKey: ["schedulerstatus", currentUser.companyId] });
        });

        connection.on("ScheduledJobUpdated", (scheduledJobId: string) => {
            queryClient.invalidateQueries({ queryKey: ["scheduledjob", scheduledJobId] });
            queryClient.invalidateQueries({ queryKey: [`scheduledjob:history:${scheduledJobId}`] });
        });

        startCompanyConnection(connection, currentUser.companyId);

        return () => {
            connection.off("ProductsUpdated");
            connection.off("ProductKpisUpdated");
            connection.off("SchedulerStatusUpdated");
            connection.off("JobTaskUpdated");
            connection.off("JobTasksUpdated");
            connection.off("ScheduledJobUpdated");
            leaveCompanyGroup(connection, currentUser.companyId);
        };
    }, [queryClient, currentUser?.companyId]);
}