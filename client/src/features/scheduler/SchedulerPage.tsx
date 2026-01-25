import { Box, Tabs, Tab } from "@mui/material";
import { useState } from "react";
import TabPanel from "../../components/tabs/TabPanel";
import SchedulerOverviewTab from "./tabs/SchedulerOverviewTab";
import RecentHistoryTab from "./tabs/RecentHistoryTab";

export default function SchedulerPage() {
  const [tab, setTab] = useState(0);

  return (
    <Box>
      <Tabs value={tab} onChange={(_, v) => setTab(v)}>
        <Tab label="Scheduler overview" />
        <Tab label="Job execution history" />
      </Tabs>

      <TabPanel value={tab} index={0}>
        <SchedulerOverviewTab />
      </TabPanel>

      <TabPanel value={tab} index={1}>
        <RecentHistoryTab />
      </TabPanel>
    </Box>
  );
}
