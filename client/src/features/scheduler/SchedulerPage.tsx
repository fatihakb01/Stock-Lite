import { Box, Tabs, Tab } from "@mui/material";
import { useState } from "react";
import TabPanel from "../../components/tabs/TabPanel";
import SchedulerOverviewTab from "./tabs/SchedulerOverviewTab";
import RecentHistoryTab from "./tabs/RecentHistoryTab";
import { useSchedulerStatus } from "../../hooks/useProductKpis";
import SchedulerStatusBadge from "./components/SchedulerStatusBadge";

export default function SchedulerPage() {
    const [tab, setTab] = useState(0);
    const { data: schedulerStatus } = useSchedulerStatus();

    return (
        <Box sx={{ m: "auto" }}>
            <Box sx={{ display: "flex", alignItems: "center" }}>
                <Tabs value={tab} onChange={(_, v) => setTab(v)} sx={{ flexGrow: 1 }}>
                    <Tab label="Scheduler overview" sx={{ ml: "auto" }} />
                    <Tab label="Job execution history" sx={{ mr: "auto" }} />
                </Tabs>
                <Box sx={{ pr: 2 }}>
                    <SchedulerStatusBadge status={schedulerStatus} label={"Scheduler"} />
                </Box>
            </Box>

            <TabPanel value={tab} index={0}>
                <SchedulerOverviewTab />
            </TabPanel>

            <TabPanel value={tab} index={1}>
                <RecentHistoryTab />
            </TabPanel>
        </Box>
    );
}