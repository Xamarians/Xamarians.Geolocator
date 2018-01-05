# Xamarians.Geolocator
Cross platform library to get current location and to continuesly listen to location

First install package from nuget using following command -
## Install-Package Xamarians.GPS -Version 1.0.2

You can integrate locator in you Xamarin Form application using following code:

 Shared Code -
 
 To set accuracy-
 ```c#
  GPSService.Instance.DesiredAccuracy = 100;
```
It get current position-
```c#
var postion = await GPSService.Instance.GetPositionAsync(CancellationToken.None);
```
To start listening to position continuously-
```c#
 GPSService.Instance.StartListening(1000, 5);
```
To stop listening to position-
```c#
 GPSService.Instance.StopListening();
```
To handle when position is changed
```c#
  GPSService.Instance.PositionChanged += Instance_PositionChanged;
```
Android - in MainActivity file write below code -
```c#
Xamarians.GPS.Droid.GPSServiceAndroid.Initialize(this);
```

iOS - in AppDelegate file write below code -
```c#
Xamarians.GPS.iOS.GPSServiceIOS.Initialize();
```
Also add following permissions in Android.
```c#
ACCESS_FINE_LOCATION
ACCESS_COARSE_LOCATION
ACCESS_NETWORK_STATE
ACCESS_WIFI_STATE
```
And following permissions in iOS.
```c#
Location Always Usage Description
Privacy - Location Usage Description
```


### For any issue with library please report here 
https://github.com/Xamarians/Xamarians.Geolocator/issues/new
