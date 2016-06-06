Series-Tracker
==============

Series Tracker is a windows phone app that helps users keep track of all their favourite series.

New features:
- By pressing '...' and going to the settings, you can now choose to have the traditional followed series list, or to sort them alphabetically.
- If you enable the "background agent" in the settings page, you can now set alarm reminders for when a new episode is out! Just go to the page of the series you want to set the alarm for and press on the little clock icon at the bottom.

User interface:
- We fixed a bug that had the series episodes in the wrong order. They should now always appear in the correct order, latest episodes at the top.
- To mark an entire series or season as seen, go into the episodes list of that series and tap&hold on a season number. Then you can select it from the context menu that will appear.
- We added the date each episode was first aired, for your convenience. Note that usually this date is in the US format (Month-Day-Year).
- Fixed links in the "About" screen, and added my twitter tag!
- Episodes that have not aired yet will no longer be counted as "not watched" in your followed series list.


Hate missing your favourite TV shows? Series Tracker helps you keep track of the TV series and shows you watch! Know exactly what and when to watch it! Can't figure out which was the last episode you watched? Wanting to know when the new episode is coming out? Then Series Tracker is for you!

If you are wondering how to do something or if a feature exists, please feel free to e-mail us or talk to us on twitter/facebook. We are more than happy to help!

E-mail: seriestracker@outlook.com
Twitter: @alexgouv
Facebook: https://www.facebook.com/SeriesTracker

Changes list etc
---------------------

* Added Google Analytics SDK.

Change entire description, and add 'series notifier' as a search term - fixed

* General refactoring - done
* Fixed episode description overflowing UI bug - fixed
* Fix HyperlinkButtons in About page, add twitter detals - fixed
* Episodes NOT in order. - fixed
* Add dates next to episodes - fixed
* Make 'following' tab have some sort of quick-search (e.g. with letters) - fixed
* Make only new episodes show in 'to see' and not all (like planned) - fixed
* FIXED FONT SIZE IN FOLLOWED VIEW SO THAT LARGE DATES DON'T OVERFLOW (e.g. 10/13/2014) - fixed
* Add settings for sorted or unsorted 'followed' view - fixed
* When selecting a season from the jumplist, the context menu option "select season as seen/not seen" does not work anymore. If one scrolls to a season instead of jumping to it, the context menu option it does work! - fixed

Backporting to wp7.1:
- notifications - fixed
- alphabetical ordering - FIXED
- unseen episode count - fixed
- about page - fixed
- add dates next to episodes, correct order - fixed
- jumplist bug - fixed

Fixed bugs:
- Completely solved ordering problem, and should not crash if the string can't be converted to an int

Known bugs:
- Alphabetical view doesn't immediately update when a series is added, need to close and re-open app

Ideas for new features
-------------------------

* Spanish localisation (mexico biggest market + all the other spanish speaking markets)
* Episode details always scroll to the top 'seen' episode
* Tutorial + welcome screen
* Lost series section - tell you what you missed last week from your favorites?
* Today or This Week section - what's on today?
* Recommend new series - explore section
* Add donations option, so that we get moneyz!
* Add some sharing options e.g. new episode is out!
* Make episode description text not as long
* "Wish it also listed networks"
* Live tiles
* win8 equivalent
