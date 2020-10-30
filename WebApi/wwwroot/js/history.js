function formatDates(arr) {
    for (let i = 0; i < arr.length; i++) {
        arr[i].BackupDateStart = formatDate(arr[i].BackupDateStart);
        arr[i].BackupDateEnd = formatDate(arr[i].BackupDateEnd);
    }
}

new Vue({
    el: '#app',
    vuetify: new Vuetify(),
    data() {
        return {
            headers: [
                { text: 'Backup Start', align: 'center', value: 'BackupDateStart' },
                { text: 'Backup End', value: 'BackupDateEnd', align: 'center' },
                { text: 'Status', value: 'Success', align: 'center' },
                { text: 'Last Known Status', value: 'LastKnownStatus', align: 'center' },
                { text: 'Exported To Folder', value: 'ExportedToFolder', align: 'center' },
                { text: 'Archived To File', value: 'ArchivedToFile', align: 'center' },
            ],
            historyRecords: [],
            hypervisorName: '',
            isLoading: true
        }
    },
    created() {
        this.getHistory();
    },
    methods: {
        getHistory() {
            let vmId = findGetParameter("vmid");
            if (vmId === null) {
                console.error("Could not find GET parameter \"vmId\".");
                return;
            }

            axios
                .get(`${getAppUrl()}/api/GetVmHistory`, {
                    params: {
                        vmid: vmId
                    }
                })
                .then(response => {
                    if (response.data.success) {
                        let data = JSON.parse(response.data.data);
                        this.hypervisorName = data.Hypervisor;
                        this.historyRecords = data.HistoryRecords;

                        formatDates(this.historyRecords);

                        this.isLoading = false;
                    } else {
                        console.error(response.data.message);
                    }
                })
                .catch(error => {
                    console.error(error);
                })
        },
        extractFileName(str) {
            return str.split('\\').pop().split('/').pop();
        },
        getColor(success) {
            if (success)
                return 'green'
            else
                return 'red'
        }
    }
})
