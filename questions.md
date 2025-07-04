# Questions

## How did you approach solving the problem?

How to solve the main problem of the exercise was clearly explained in README.md. 
So the actual problem was to find out how each particular step needed to be implemented. 
For example, partial download was new to me, so I did not expect http status code 206, but rather 200, i.e., it failed first time.
Or, I spend some time, wondering why .AcceptRanges is a collection. In retrospect, I shouldn't have worried about that and just accepted the fact.

*Update on 14 Sept, 11:00pm:*  
My initial attempt to verify MD5 failed, when I compared hash from the headers with the hash generated directly from the stream's byte array. Only after I read the file back from disk, the hash matches. I haven't looked into this, but I am guessing, there may be something like EOF byte involved. 

## How did you verify your solution works correctly?

I tested both partial and regular downloads. 
The web server serving https://installerstaging.accurx.com/chain/3.94.56148.0/accuRx.Installer.Local.msi supports partial download, so in order to test regular (non-partial) download, I hardcoded flag bUsePartialDownloader = false.
Because, it takes more than 5 seconds to download, I did not have to use netlimiter to test cancellation: I had enough time to cancel it before it finishes.
I knew that the downloaded files were intact, because i could run the installer (I did not actually install the app, but cancel), so i postponed doing MD5 check.

Come to think about it - I do need to test internet failure, for which I might need netlimiter (15 quid). Maybe, tomorrow.

## How long did you spend on the exercise?

I downloaded the code late on Wednesday, but I did not have time to look at the code until Thursday evening, and then I also read the exercise description.
After that I, sort of involuntarily, thought about the problem, probably, for about 30 minutes on Thursday and Friday each, but did not do any coding.
On Saturday, today that is, I worked on this for about 5 hours (6 hours with a break). 

*Update on Tuesday 14 Sept, 11:00pm:*  
So far:  

Day | Spent | Result
--- | --- | ---
Saturday | 5 | Both partial and "in-one-go" downloads are working
Sunday | 2 | Tested internet cut: download fails
Monday | 1 | Partial download succeeds when internet disappears temporarily
Tuesday | 1.5 | Added MD5 check. Also noticed and fixed "remaining time"

## What would you add if you had more time and how?

I need to test internet failure.

I did not do MD5 checking - this should simple, because I can see the header:  
&nbsp;&nbsp;``Content-MD5: L23uJOgrCAbOQpiduHioTg==``

I did not add unit testing, which I hope to do tomorrow, regardless of whether it'll count or not

I would add app.config and put url, filename, and potentially a few other parameters in there.

I would also refactor the cancellation.


## Update on 13 Sept, 9:38am

Yesterday evening I disabled my wifi adapter in order to test internet cuts, and discovered, that httpclient gets into a deadlock.
I could not fix that using client.Timeout or cancelTokenSource.CancelAfter(...). I spent two sad hours trying things with it, but no luck.
The only positive outcome of this, was that I saw that my   
&nbsp;&nbsp;``while (!successForThisChunk)``  
statement should have been a few lines higher.

This morning, I tried WebClient instead, and that worked. It's now tolerating me disabling wifi adapter.

All the other mentioned items are outstanding

## Update on 14 Sept, 11:00pm

Added MD5 check; Fixed remaining time estimate.

Time spent: it's been a couple of hours, but I was distracted and went away multiple times. I'd say somewhat than an hour.
