using log4net;
using Mono.Addins;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Region.Framework.Scenes;
using System.Reflection;
using Nini.Config;
using OpenSim.Framework;
using OpenMetaverse;
[assembly: Addin("AbuseReportsModule", "1.0")]
[assembly: AddinDependency("OpenSim.Region.Framework", OpenSim.VersionInfo.VersionNumber)]
namespace AbuseReports
{
    [Extension(Path = "/OpenSim/RegionModules", NodeName = "RegionModule", Id = "AbuseReportsModule")]
    public class AbuseReports : ISharedRegionModule
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private bool m_Enabled = false;
        private string ABURL = null;

        public void Initialise(IConfigSource source)
        {
            IConfig conf = source.Configs["AbuseReports"];
            if (conf == null)
                return;
            if (conf != null && conf.GetString("AbuseReportModule", string.Empty) != this.Name)
                return;

            m_Enabled = conf.GetBoolean("Enabled", false);
            ABURL = conf.GetString("ABURL");
            m_log.InfoFormat("[AbuseReports]: Initializing {0}", this.Name);
        }
        #region IShareCrap
        private List<Scene> m_sceneList = new List<Scene>();
        public Type ReplaceableInterface
        {
            get { return null; }
        }

        public string Name
        {
            get { return "AbuseReports"; }
        }

        public void AddRegion(Scene scene) {
            //
        }
        public void Close() {
            //
        }
        public void PostInitialise() {
            //
        }
        public void RemoveRegion(Scene scene)
        {
            if (!m_Enabled)
                return;

            scene.EventManager.OnNewClient -= NewClient;
            scene.EventManager.OnClientClosed -= OnClientClosed;
            lock (m_sceneList)
            {
                m_sceneList.Remove(scene);
            }
        }

        public void RegionLoaded(Scene scene)
        {
            if (!m_Enabled)
                return;

            lock (m_sceneList)
            {
                m_sceneList.Add(scene);
            }
            scene.EventManager.OnNewClient += NewClient;
            scene.EventManager.OnClientClosed += OnClientClosed;
        }

        private void OnClientClosed(UUID clientID, Scene scene)
        {
            if (scene == null)
                return;

            ScenePresence sp = scene.GetScenePresence(clientID);
            IClientAPI client = sp.ControllingClient;
            if (client != null)
            {
                client.OnUserReport -= OnDoReport;
            }
        }

        private void NewClient(IClientAPI client)
        {
            if (!m_Enabled)
                return;

            client.OnUserReport += OnDoReport;
        }
        #endregion
        private void OnDoReport(IClientAPI iclient, 
            string regionName, 
            UUID abuserID, 
            byte catagory, 
            byte checkflags, 
            string Details, 
            UUID objectID, 
            Vector3 postion, 
            byte reportType, 
            UUID screenshotID, 
            string Summary, 
            UUID reporter)
        {

            m_log.Info("[AbuseReport] Attempting to send a abuse report");

            Dictionary<string, string> map = new Dictionary<string, string>();
            map.Add("summary", Summary);
            map.Add("details", Details);

            Task<bool> isSent = SendData(map);
            if (isSent.Result == true)
            {
                m_log.Info("[AbuseReport] Report successfully sent");
            }
            else
            {
                m_log.DebugFormat("[AbuseReport] Unable to send data to {0}", ABURL);
            }
        }
        private async Task<bool> SendData(Dictionary<string, string> data)
        {
            bool sent = false;
            using (HttpClient client = new HttpClient()) {

                var stringContent = new FormUrlEncodedContent(data);
                using (HttpResponseMessage response = await client.PostAsync(ABURL, stringContent))
                {
                    using (HttpContent content = response.Content)
                    {
                        string reply = await content.ReadAsStringAsync();
                        if (reply == "ok")
                        {
                            sent = true;
                        }
                    }
                }
            }
            return sent;
        }
    }
}
