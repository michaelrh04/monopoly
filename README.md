# Monopoly project

This project to re-create the board game **Monopoly** was completed for the A-level computer science coursework component (AQA) for the 2021 exam year. The solution is written ultimately as a Windows Presentation Foundation (WPF) app, using XAML, with the large majority of functionality handled and executed with C# view-models and models (the application operates along the MVVM pattern).

## Important notices
### Loading a game for the first time (required step)
For the operation of this game, the default board (included in the files above) **must be moved to the correct directory**. As of the latest update to README.md, this directory was contained within the default documents folder, at _Documents\Monopoly\Boards_. The default board included with the solution is entitled *UK classic board.mboard*.

You can create this directory yourself or allow the application to create it for you (it should do so upon first launching the program), but you will not be able to select a board (and, hence, you will be unable to start the game) if this directory is empty. **Note that the game does not automatically create a default board for you. This is a required step to play any game.**

### The use of packages
This project uses NuGet packages (MahApps and MahApps icons) to provide skins for the default WPF controls, and (as a result) will not work/will look incredibly interesting without these. On some systems, *through no fault of the program solution itself*, it appears that the Nuget package BouncyCastle does not load correctly upon downloading and extracting the source code: an easy fix for this is to simply navigate to 'Manage NuGet packages' and re-install the package. 
