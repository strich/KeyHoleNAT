namespace KeyHole {
    public class Module {
        public delegate void ProgressUpdateHandler(object sender, ProgressUpdateEventArgs e);

        public event ProgressUpdateHandler ProgressUpdate;
        public event ProgressUpdateHandler ProgressFinished;

        protected void OnProgressUpdate(ProgressUpdateEventArgs e) {
            if (ProgressUpdate != null) {
                ProgressUpdate(this, e);
            }
        }

        protected void OnProgressUpdate(string update) {
            OnProgressUpdate(new ProgressUpdateEventArgs {
                MessageDescription = update
            });
        }

        protected void OnProgressFinished(ProgressUpdateEventArgs e) {
            if (ProgressUpdate != null) {
                ProgressUpdate(this, e);
            }
        }
    }
}
