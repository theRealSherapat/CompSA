# MSc Thesis writing

Remember:
* "Less is more." — experience and William Strunk Jr.
* "Don't overkill it." — experience (crazy and many color-codes)
* "Ikke drible eller lure deg selv." — overdreven bruk av "clarifying markers" og fotball


## Specific TODOs
* Synkroniser "StrukturConcerns" med de to andre "SporsmaalAaSvarePaa" og "ContentSuggestions".
* Fiks ordentlig kode-lint i Notepad++ sånn at jeg sparer mye tid og tekst-klassifiserings-krefter (ihvertfall for Python).
* Execute 'GJØR: []'-clauses possible to execute now (remember Abe Lincoln).
* Gå igjennom 'notert-på reMarkable Thesis-document(s)' og 'Essay .tex'-fila, så rett på eller hent kritikk, INKL.:[]?'s, og concerns.
* Skriv mot Up-To-Date MSc-thesis /-chapter Draft.




## Polishing / Finishing TODOs
* Polish/finish font-families, -styles, and -sizes (by e.g. using the packages 'sectsty' or 'titlesec' — but preferably just LaTeX's standard font-manipulation if possible):
	+ Make Section- & Chapter-headlines (and possibly the 2nd thesis-title, like in the Overleaf-IFI-standard-document) bold-face.
	+ Ensure that the Thesis-title isn't inferior and smaller perhaps than the Chapter/Section-headlines.
	+ Use a good-looking font-family. Previously looked at font-families are:
		- 'Aldus Nova Book' by Linotype (found through myfonts.com/WhatTheFont with a sample from Tønnes's master).
		- 'bold Adobe Times' a.k.a. '\fontfamily{ptm}'  (found through the 'sectsty'-package docs on page 3).
		- 'Libre Baskerville' a.k.a. '\usepackage{librebaskerville} \usepackage[T1]{fontenc}' (found at tug.org/FontCatalogue/seriffonts.html).
	+ Be consistent about using italization on variables and words of emphasis, but not on operators (in equations).
	+ Use bold face on Figure-captions (so that it's rather **Figure 4.1** instead of Figure 4.1).
	+ Minimize the use of parentheses and '—'s.
* Polish/finish citations and references throughout the master's report:
	+ Add the right citations/references where there are empty \cite{}-clauses.
	+ Ensure the right uses of \ref{}, so that 'Subsection \ref{}' doesn't e.g. refer to a Section or Paragraph.
	+ Change to using the [NOR04]-style (first author(s), Normann e.g.) instead of [3]-style (cf. https://www.overleaf.com/learn/latex/Bibliography_management_in_LaTeX).
* Polish/finish the page layouts:
	+ Se til at alle figurene og tabellene er plassert der det gir mening for deg, ikke der det gir mening for LaTeX.

## Pre-Korrekturlesning TODOs
* Høre med Kyrre om det er fryktelig nøye åssen referansene er skrevet opp, eller om Zoteros eksporterte .bib-filer er gode nok.


#### Possible setup While having displayable writing-inspirations in mind:
Possibly a good workflow is preparing the set-up:
1) BIG SCREEN AVAILABLE:
* On the big screen opening and having on the right or left side:
	+ (On the left side) A thesis writing-tips document (e.g. from Jim Tørresen) and your own MSc document
	+ (On the right side) Good MSc theses examples (as many as is needed or useful, in the order 'Nygaard, Samuelsen, Ruud')
* On the smaller laptop-screen:
	+ Notepad++ with the master.tex-document, this README, the various Chapters for the master.tex that you're working on
2) ONLY LAPTOP AVAILABLE:
* Think of something you want to take inspiration from or compare to, w.r.t. something you could write about right now (something that's on your mind)
	+ View the inspiration in a top-left corner of the window
	+ Open Notepad++ and your MSc-document in the middle
	+ View your resulting MSc-PDF in the bottom right corner of the window