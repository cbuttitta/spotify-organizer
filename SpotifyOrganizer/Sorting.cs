namespace SpotifyOrganizer
{
    public class Sorter()
    {

    }
}

/* workflow
 1. get the users profile
 2. get the user's playlists and display it to them pretty print like:
    1. Playlist Name: (bold)
    Track 1 (italic)
    Track 2
    Track 3
 3. ask if there are any playlists they would like to exclude by number
 4. add to a skip list these numbers if given
* 1. create Dictionary of genres with lists of track ids
* 1. get the tracks one by one from the playlist
* 2. get the genre of the song
*    2a. get the artist of the song
*    2b. get the genre associated with the artist
*    2c. for every genre that comes up, add the track id to that genre list
* 3. if the genre doesn't exist yet, create an entry for it, otherwise add it to the list of ids
* 4. once gone through the playlist, go to the next playlist
* 5. repeat until all playlists are gone through
* 6. then pretty print proposed playlists to user and ask if they would like to go forward with the operation
* 7. if not, abort program. if yes, create playlists named after genre and populate with all the tracks in the list
* 
* 
* 
*
* 
* 
*/