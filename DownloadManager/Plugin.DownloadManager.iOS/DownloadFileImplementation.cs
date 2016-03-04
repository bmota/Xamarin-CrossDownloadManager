﻿using System.Collections.Generic;
using System.ComponentModel;
using Foundation;
using Plugin.DownloadManager.Abstractions;

namespace Plugin.DownloadManager
{
    public class DownloadFileImplementation : IDownloadFile
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /**
         * The task, running in the background
         */
        public NSUrlSessionDownloadTask Task;

        public string Url { get; private set; }

        public IDictionary<string, string> Headers { get; private set; }

        DownloadFileStatus _status;

        public DownloadFileStatus Status {
            get {
                return _status;
            }
            set {
                if (!Equals (_status, value)) {
                    _status = value;
                    PropertyChanged?.Invoke (this, new PropertyChangedEventArgs ("Status"));
                }
            }
        }

        float _totalBytesExpected = 0.0f;

        public float TotalBytesExpected {
            get {
                return _totalBytesExpected;
            }
            set {
                if (!Equals (_totalBytesExpected, value)) {
                    _totalBytesExpected = value;
                    PropertyChanged?.Invoke (this, new PropertyChangedEventArgs ("TotalBytesExpected"));
                }
            }
        }

        float _totalBytesWritten = 0.0f;

        public float TotalBytesWritten {
            get {
                return _totalBytesWritten;
            }
            set {
                if (!Equals (_totalBytesWritten, value)) {
                    _totalBytesWritten = value;
                    PropertyChanged?.Invoke (this, new PropertyChangedEventArgs ("TotalBytesWritten"));
                }
            }
        }

        /**
         * Initializing a new object to add it to the download-queue
         */
        public DownloadFileImplementation (string url, IDictionary<string, string> headers)
        {
            Url = url;
            Headers = headers;

            Status = DownloadFileStatus.PENDING;
        }

        /**
         * Called when re-initializing the app after the app shut down to be able to still handle on-success calls.
         */
        public DownloadFileImplementation (NSUrlSessionDownloadTask task)
        {
            Url = task.OriginalRequest.Url.AbsoluteString;
            Headers = new Dictionary<string, string> ();

            foreach (var header in task.OriginalRequest.Headers) {
                Headers.Add (new KeyValuePair<string, string> (header.Key.ToString (), header.Value.ToString ()));
            }

            Status = DownloadFileStatus.PENDING;

            switch (task.State) {
            case NSUrlSessionTaskState.Running:
                Status = DownloadFileStatus.RUNNING;
                break;
            case NSUrlSessionTaskState.Completed:
                Status = DownloadFileStatus.COMPLETED;
                break;
            case NSUrlSessionTaskState.Canceling:
                Status = DownloadFileStatus.RUNNING;
                break;
            case NSUrlSessionTaskState.Suspended:
                Status = DownloadFileStatus.PAUSED;
                break;
            }

            Task = task;
        }

        public void StartDownload (NSUrlSession session)
        {
            using (var downloadURL = NSUrl.FromString (Url))
            using (var request = new NSMutableUrlRequest (downloadURL)) {
                if (Headers != null) {
                    var headers = new NSMutableDictionary ();
                    foreach (var header in Headers) {
                        headers.SetValueForKey (
                            new NSString (header.Value),
                            new NSString (header.Key)
                        );
                    }
                    request.Headers = headers;
                }

                Task = session.CreateDownloadTask (request);
                Task.Resume ();
            }
        }
    }
}