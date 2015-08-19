# Azure DNS Manager

Quick UI based DNS manager for Azure DNS using Azure .NET Api thru NuGet.

## Features

On startup of the program the system will try to establish a connection with Azure thru credentials optionally 
cached on the system. You should authenticate with a Azure account. Initial setup of Azure DNS by creating 
a resource group is not implemented in this software but can be accomplished with some Powershell commands.

https://azure.microsoft.com/en-us/documentation/articles/dns-getstarted-create-dnszone/

The program automatically selects your subscription if only one could be found and the resourcegroup name. 
Then a list of currently registered DNS zones will be retrieved. Finally it displays the first zone from that list
on the right hand side.

## Making changes

To make changes to existing records, simply select the record (a "commit" button will show on the right hand size of
the record line) and update the values. Click on the commit button and wait, the commit button should disappear and
your changes are Live! (It's that simple!!!)

Adding new zones is super simple to, just enter the name of the zone when clicking "Add" below the zone list and 
wait for it to appear on the left hand side.

Adding new records is easy too, just click on the corrosponding add button for the record you want to add.

Enjoy using Azure DNS!

## Binary

Find a binary of the program here: http://scox.nl/azure_dns_manager_v0.1.zip

## Todo

- Deletion of zone

- Nicer dialogs for adding new records
- Clearer notice on how to commit

- Strip long lines or add scroll lines within record text (especially for DKIM TXT records)

## Copyright

Copyright Sander Cox
Licensed under GPLv3
