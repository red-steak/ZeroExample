# Zero Example

An Example of the WPF application - grouping XML data 

Used RectiveUI -> https://www.reactiveui.net/

**Projects**

Models      => class Car

Interfaces  => interfaces for DI

Services    => services for DI used in the app; references Models, Interfaces

ViewModels  => viewmodels (UI logic); references Models, Interfaces

ZeroExample => WPF application; references ViewModels, Interfaces, Services