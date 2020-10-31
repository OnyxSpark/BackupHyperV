# BackupHyperV
A Windows service that backs up Microsoft Hyper-V virtual machines.

### Description

The system consists of 2 parts: local agents and an optional central server. The agent can be launched either from the command line with administrator rights, or it can be installed on the system as a service, for example, with the following command:

```
sc.exe create BackupHyperV binPath= C:\BackupHyperV\BackupHyperV.Service.exe
```

At startup, the agent tries to find the `BackupTask.json` file, which contains the rules for backing up virtual machines. If the file cannot be found, a new file will be created that will list all current virtual machines and create default settings for them. In order for the backups to work, you need to edit the file (change the paths of the backups and enable backups of those machines where necessary).

The backup process itself consists of 2 parts:

1. Exporting virtual machine files to the specified folder.
2. Optionally compressing this folder into a zip archive.

The central server is responsible for the following aspects:

1. Shows the current state of all agents connected to it.
2. Keeps a history of backups of virtual machines.
3. Allows you to change the backup settings of each machine separately - the backup schedule, the path to the backup file, etc.

At this moment, the first 2 points have been implemented:

Current state of agents:

![MainPage](https://github.com/OnyxSpark/BackupHyperV/blob/master/Images/MainPage.jpg "Main page")

Virtual machine backup history:

![HistoryPage](https://github.com/OnyxSpark/BackupHyperV/blob/master/Images/HistoryPage.jpg "History page")

