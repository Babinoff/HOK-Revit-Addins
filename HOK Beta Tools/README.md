# HOK Beta Addin Manager

#### This tool dynamically generates Ribbon Tab items based on user selection.

###### Release 2018.0.0.2

* Since Mission Control tool has things that subscribe to events, and dockable panels, it requires to be properly loaded in on startup
to not crash. To accomodate that all new installed tools, are disabled, until user restarts Revit. 
* Added new "sysvol" based location for the beta tools to speed up the process of mirroring. 
* Beta installer will also now compare the contents of the current temp directory against what is installed in the user addins directory to identify any `HOK.` plugins that might be discontinued and delete them. 
* Minor UI fixes.

###### Release 2018.0.0.1

* Total revamp of what Jinsol was doing. It's now using class attributes to extract all required information from each beta DLL
in order to generate the ribbon items and populate WPF UI. 
