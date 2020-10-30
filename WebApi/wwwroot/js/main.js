function getAppUrl() {
    return `${window.location.protocol}//${window.location.host}/BackupHyperV`;
}

function formatDate(value) {
    if (value) {
        return moment(String(value)).format('DD.MM.YYYY HH:mm:ss')
    }
}

function findGetParameter(parameterName) {
    let result = null, tmp = [];
    let items = location.search.substr(1).split("&");

    for (let index = 0; index < items.length; index++) {
        tmp = items[index].split("=");
        if (tmp[0] === parameterName)
            result = decodeURIComponent(tmp[1]);
    }

    return result;
}
