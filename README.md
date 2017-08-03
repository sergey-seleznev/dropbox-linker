# Short description

The goal of DropBox Linker project is to improve DropBox service client usability.  
It monitors changes to published files and intellectually copies their URLs to the clipboard.

# Detailed functions overview

Assume you publish a file with DropBox:  
![alt text](https://github.com/sergey-seleznev/dropbox-linker/blob/master/doc/images/copy-to-public.png "Copy to public")  

The standard way to get the link is to open Public folder and fire an item of file's context menu:  
![alt text](https://github.com/sergey-seleznev/dropbox-linker/blob/master/doc/images/copy-public-link.png "Copy public link")  

I've found this activity very annoying. The decision was to automate it in a simple and beautiful way.  
The solution needs to know your User ID. Together with other options, it has to be set in the settings window:  
![alt text](https://github.com/sergey-seleznev/dropbox-linker/blob/master/doc/images/settings.png "Settings")  

While this utility running in the background, you will get the link automatically copied to clipboard  
and receive the corresponding visual notification:  
![alt text](https://github.com/sergey-seleznev/dropbox-linker/blob/master/doc/images/add.png "Add")  

However, it is the usual thing to publish multiple files at once:  
![alt text](https://github.com/sergey-seleznev/dropbox-linker/blob/master/doc/images/multi-copy-to-public.png "Multiple copy to public")  

To get the complete list of links you had to iterate all the files and copy their URLs.  
However, the utility appends new links to the clipboard, if it contains other URLs.  
Links are guaranteed not to be duplicated, if already exists in the clipboard.  
The popup looks perfect with a dark background as well.  
![alt text](https://github.com/sergey-seleznev/dropbox-linker/blob/master/doc/images/multi-add.png "")  

If you rename a file within your Public folder, and the file was already referenced  
by a URL in your clipboard, its link will be corrected corresponding to the last version.  

Additionally, if you move referenced a file out of Public folder (or simply delete it),  
its URL will be safely removed from the clipboard.  
![alt text](https://github.com/sergey-seleznev/dropbox-linker/blob/master/doc/images/remove.png "Remove")  

DropBox Linker allows you to perform selective URL copying as well.  
Just check the required files, click a button and paste your links!  
![alt text](https://github.com/sergey-seleznev/dropbox-linker/blob/master/doc/images/get-links.png "Getting links")  
To make it even more comfortable, live filtering is implemented with a slider.  
Additionally, you may change the order your files are listed in the file browser.  