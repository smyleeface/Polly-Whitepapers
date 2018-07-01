Proccess PDF to Text
====================

## Prerequisites
* Python3 & pip3
* Install requirements.txt

## Run
* `cd pdftotext`
* `python process.py`
* `python process_output.py`

## What it does

### process.py
* Reads file `AWS_Well-Architected_Framework.pdf`
* Processes the pages from hex to binary
* Uses regex to get the text
* Prints each page to screen
* Outputs text into single file

### process_output.py
* Uses code from http://denis.papathanasiou.org/archive/2010.08.04.post.pdf with minor changes to support pdfminer.six
* Prints each page to screen in a nice format

## Todos
* Need a better way of extracting the text
* Find a way to tell what the formatting is
