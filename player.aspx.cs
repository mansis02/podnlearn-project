using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;

namespace PodnLearn
{
    public partial class player : System.Web.UI.Page
    {
        protected string PodcastFilePath { get; set; }
        protected string ThumbPath { get; set; }

        string connStr2 = ConfigurationManager.ConnectionStrings["connStrPod"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // ✅ Check login
                if (Session["Username"] == null)
                {
                    Response.Redirect("login.aspx");
                    return;
                }

                // ✅ Get episode name safely
                string episodeName = Request.QueryString["episodeName"];

                if (string.IsNullOrEmpty(episodeName))
                {
                    Response.Redirect("Default.aspx");
                    return;
                }

                // ✅ Set values
                Session["EpisodeName"] = episodeName;
                epName.Text = episodeName;
                epNameAgain.Text = episodeName;

                // ✅ Load data
                LoadPodcastData(episodeName);

                // ✅ Check favorites
                CheckFavorite();
            }
        }

        // ✅ NEW CLEAN METHOD
        private void LoadPodcastData(string episodeName)
        {
            using (SqlConnection con = new SqlConnection(connStr2))
            {
                con.Open();

                string query = "SELECT podcastName, EPName, epFilepath FROM PodInfo WHERE EPName = @epname";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@epname", episodeName);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            podName.Text = reader["podcastName"].ToString();
                            podNameAgain.Text = reader["podcastName"].ToString();

                            epName.Text = reader["EPName"].ToString();
                            epNameAgain.Text = reader["EPName"].ToString();

                            PodcastFilePath = reader["epFilepath"].ToString();
                        }
                        else
                        {
                            epName.Text = "Podcast not found!";
                            return;
                        }
                    }
                }
            }

            // ✅ Get thumbnail
            using (SqlConnection con = new SqlConnection(connStr2))
            {
                con.Open();

                string query = "SELECT podcastID, thumbnailPath, authorName FROM PodcastDetails WHERE podcastName = @podName";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@podName", podName.Text);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            ThumbPath = reader["thumbnailPath"].ToString();
                            Session["podcastID"] = reader["podcastID"].ToString();
                            Session["authorName"] = reader["authorName"].ToString();
                        }
                    }
                }
            }
        }

        // ✅ Favorite check
        private void CheckFavorite()
        {
            string query = "SELECT COUNT(*) FROM Favorites WHERE Username = @user AND EPName = @epName";

            using (SqlConnection con = new SqlConnection(connStr2))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@user", Session["Username"].ToString());
                    cmd.Parameters.AddWithValue("@epName", epName.Text);

                    con.Open();
                    int count = (int)cmd.ExecuteScalar();

                    if (count > 0)
                    {
                        addLabel.Text = "In Favorites";
                        addLabel.ForeColor = System.Drawing.Color.LightBlue;
                        toggleFavoriteButton.Text = "❤️";
                    }
                    else
                    {
                        addLabel.Text = "Add to Favorites";
                        toggleFavoriteButton.Text = "🤍";
                    }
                }
            }
        }

        // ❤️ Toggle favorite
        protected void toggleFavoriteButton_Click(object sender, EventArgs e)
        {
            ToggleFavorite();
            Response.Redirect(Request.Url.AbsoluteUri);
        }

        private void ToggleFavorite()
        {
            string username = Session["Username"].ToString();
            string podcastID = Session["podcastID"].ToString();
            string episodeName = epName.Text;
            string authorName = Session["authorName"].ToString();

            if (IsEpisodeInFavorites(username, podcastID))
            {
                RemoveFromFavorites(username, podcastID);
            }
            else
            {
                AddToFavorites(username, podcastID, episodeName, authorName);
            }
        }

        private bool IsEpisodeInFavorites(string username, string podcastID)
        {
            string query = "SELECT COUNT(*) FROM Favorites WHERE Username = @user AND podcastID = @podID";

            using (SqlConnection con = new SqlConnection(connStr2))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@user", username);
                    cmd.Parameters.AddWithValue("@podID", podcastID);

                    con.Open();
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        private void AddToFavorites(string username, string podcastID, string episodeName, string authorName)
        {
            string query = "INSERT INTO Favorites (podcastId, podcastName, EPName, authorName, Username) VALUES (@podID, @podName, @epName, @author, @user)";

            using (SqlConnection con = new SqlConnection(connStr2))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@podID", podcastID);
                    cmd.Parameters.AddWithValue("@podName", podName.Text);
                    cmd.Parameters.AddWithValue("@epName", episodeName);
                    cmd.Parameters.AddWithValue("@author", authorName);
                    cmd.Parameters.AddWithValue("@user", username);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void RemoveFromFavorites(string username, string podcastID)
        {
            string query = "DELETE FROM Favorites WHERE Username = @user AND podcastID = @podID";

            using (SqlConnection con = new SqlConnection(connStr2))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@user", username);
                    cmd.Parameters.AddWithValue("@podID", podcastID);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}