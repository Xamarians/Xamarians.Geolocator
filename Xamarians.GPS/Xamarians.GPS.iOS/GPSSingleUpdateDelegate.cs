using System;
using CoreLocation;
using System.Threading.Tasks;
using System.Threading;
using Foundation;

namespace Xamarians.GPS.iOS
{

    /// <summary>
    /// Class GeolocationSingleUpdateDelegate.
    /// </summary>
    internal class GPSSingleUpdateDelegate : CLLocationManagerDelegate
    {
        /// <summary>
        /// The _best heading
        /// </summary>
        private CLHeading bestHeading;

        /// <summary>
        /// The _have heading
        /// </summary>
        private bool haveHeading;

        /// <summary>
        /// The _have location
        /// </summary>
        private bool haveLocation;

        /// <summary>
        /// The _desired accuracy
        /// </summary>
        private readonly double desiredAccuracy;

        /// <summary>
        /// The _include heading
        /// </summary>
        private readonly bool includeHeading;

        /// <summary>
        /// The _manager
        /// </summary>
        private readonly CLLocationManager manager;

        /// <summary>
        /// The _position
        /// </summary>
        private readonly Position position = new Position();

        /// <summary>
        /// The _TCS
        /// </summary>
        private readonly TaskCompletionSource<Position> tcs;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeolocationSingleUpdateDelegate"/> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <param name="desiredAccuracy">The desired accuracy.</param>
        /// <param name="includeHeading">if set to <c>true</c> [include heading].</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="cancelToken">The cancel token.</param>
        public GPSSingleUpdateDelegate(
            CLLocationManager manager,
            double desiredAccuracy,
            bool includeHeading,
            int timeout,
            CancellationToken cancelToken)
        {
            this.manager = manager;
            tcs = new TaskCompletionSource<Position>(manager);
            this.desiredAccuracy = desiredAccuracy;
            this.includeHeading = includeHeading;

            if (timeout != Timeout.Infinite)
            {
                Timer t = null;
                t = new Timer(
                    s =>
                    {
                        if (haveLocation)
                        {
                            tcs.TrySetResult(new Position(position));
                        }
                        else
                        {
                            tcs.TrySetException(new TimeoutException());
                        }

                        StopListening();
                        t.Dispose();
                    },
                    null,
                    timeout,
                    0);
            }

            cancelToken.Register(
                () =>
                {
                    StopListening();
                    tcs.TrySetCanceled();
                });
        }

        /// <summary>
        /// Gets the task.
        /// </summary>
        /// <value>The task.</value>
        public Task<Position> Task
        {
            get
            {
                return tcs.Task;
            }
        }

        /// <summary>
        /// Authorizations the changed.
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <param name="status">The status.</param>
        public override void AuthorizationChanged(CLLocationManager manager, CLAuthorizationStatus status)
        {
            // If user has services disabled, we're just going to throw an exception for consistency.
            if (status == CLAuthorizationStatus.Denied || status == CLAuthorizationStatus.Restricted)
            {
                StopListening();
                tcs.TrySetException(new GeolocationException(GeolocationError.Unauthorized));
            }
        }

        /// <summary>
        /// Failes the specified manager.
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <param name="error">The error.</param>
        public override void Failed(CLLocationManager manager, NSError error)
        {
            switch ((CLError)(int)error.Code)
            {
                case CLError.Network:
                    StopListening();
                    tcs.SetException(new GeolocationException(GeolocationError.PositionUnavailable));
                    break;
            }
        }

        /// <summary>
        /// Shoulds the display heading calibration.
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public override bool ShouldDisplayHeadingCalibration(CLLocationManager manager)
        {
            return true;
        }

        /// <summary>
        /// Updates the location.
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <param name="newLocation">The new location.</param>
        /// <param name="oldLocation">The old location.</param>
        public override void UpdatedLocation(CLLocationManager manager, CLLocation newLocation, CLLocation oldLocation)
        {
            if (newLocation.HorizontalAccuracy < 0)
            {
                return;
            }

            if (haveLocation && newLocation.HorizontalAccuracy > position.Accuracy)
            {
                return;
            }

            position.Accuracy = newLocation.HorizontalAccuracy;
            position.Altitude = newLocation.Altitude;
            position.AltitudeAccuracy = newLocation.VerticalAccuracy;
            position.Latitude = newLocation.Coordinate.Latitude;
            position.Longitude = newLocation.Coordinate.Longitude;
            position.Speed = newLocation.Speed;
            position.Timestamp = new DateTimeOffset((DateTime)newLocation.Timestamp);

            haveLocation = true;

            if ((!includeHeading || haveHeading) && position.Accuracy <= desiredAccuracy)
            {
                tcs.TrySetResult(new Position(position));
                StopListening();
            }
        }

        /// <summary>
        /// Updates the heading.
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <param name="newHeading">The new heading.</param>
        public override void UpdatedHeading(CLLocationManager manager, CLHeading newHeading)
        {
            if (newHeading.HeadingAccuracy < 0)
            {
                return;
            }
            if (bestHeading != null && newHeading.HeadingAccuracy >= bestHeading.HeadingAccuracy)
            {
                return;
            }

            bestHeading = newHeading;
            position.Heading = newHeading.TrueHeading;
            haveHeading = true;

            if (haveLocation && position.Accuracy <= desiredAccuracy)
            {
                tcs.TrySetResult(new Position(position));
                StopListening();
            }
        }

        /// <summary>
        /// Stops the listening.
        /// </summary>
        private void StopListening()
        {
            if (CLLocationManager.HeadingAvailable)
            {
                manager.StopUpdatingHeading();
            }

            manager.StopUpdatingLocation();
        }


    }
}