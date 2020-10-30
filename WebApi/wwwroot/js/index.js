function formatDates(arr) {
    for (let i = 0; i < arr.length; i++) {
        arr[i].LastBackup = formatDate(arr[i].LastBackup);
        arr[i].StatusUpdated = formatDate(arr[i].StatusUpdated);
    }
}

new Vue({
    el: '#app',
    vuetify: new Vuetify(),
    data() {
        return {
            headers: [
                { text: 'Hypervisor', align: 'center', value: 'Hypervisor' },
                { text: 'Virtual Machine', value: 'Name', align: 'center' },
                { text: 'Status', value: 'Status', align: 'center' },
                { text: 'Percent Complete', value: 'PercentComplete', align: 'center' },
                { text: 'Settings', value: '', align: 'center', sortable: false },
                { text: 'History', value: '', align: 'center', sortable: false, },
                { text: 'Last Backup', value: 'LastBackup', align: 'center' },
                { text: 'Status Updated', value: 'StatusUpdated', align: 'center' },
            ],
            virtualMachines: [],
            timer: '',
            isLoading: true
        }
    },
    created() {
        this.getVirtualMachines();
        this.timer = setInterval(this.getVirtualMachines, 5000);
    },
    methods: {
        getVirtualMachines() {
            axios
                .get(`${getAppUrl()}/api/GetVmStates`)
                .then(response => {
                    if (response.data.success) {
                        this.virtualMachines = JSON.parse(response.data.data);

                        formatDates(this.virtualMachines);

                        this.isLoading = false;
                    } else {
                        console.error(response.data.message);
                    }
                })
                .catch(error => {
                    console.error(error);
                })
        },
        onButtonSettingsClick() {
        },
        onHistoryButtonClick(vmId) {
            window.location.href = `${getAppUrl()}/history.html?vmid=${vmId}`;
        }
    },
    beforeDestroy() {
        clearInterval(this.timer);
    }
})
