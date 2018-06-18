import os
import re

from pdfminer.pdfparser import PDFParser
from pdfminer.pdfdocument import PDFDocument
from pdfminer.pdfpage import PDFPage
from pdfminer.pdfpage import PDFTextExtractionNotAllowed
from pdfminer.pdfinterp import PDFResourceManager
from pdfminer.pdfinterp import PDFPageInterpreter
from pdfminer.pdfdevice import PDFDevice


regex_pattern = "\(([A-Za-z\d -]+)\)"
file_path = os.path.dirname(os.path.abspath(__file__))
input_filename = 'AWS_Well-Architected_Framework.pdf'
output_filename = 'AWS_Well-Architected_Framework.txt'

# Open a PDF file.
fp = open(f'{file_path}/{input_filename}', 'rb')

# Create a PDF parser object associated with the file object.
parser = PDFParser(fp)

# Create a PDF document object that stores the document structure.
document = PDFDocument(parser)

# Check if the document allows text extraction. If not, abort.
if not document.is_extractable:
    raise PDFTextExtractionNotAllowed

# Create a PDF resource manager object that stores shared resources.
resource_manager = PDFResourceManager()

# Create a PDF device object.
device = PDFDevice(resource_manager)

# Create a PDF interpreter object.
interpreter = PDFPageInterpreter(resource_manager, device)

# Process each page contained in the document.
pdf_text = ''
for page in PDFPage.get_pages(fp, set(), 0, document):

    # convert hex to data
    interpreter.process_page(page)
    page_data = page.contents[0].data
    if page_data:
        m = re.findall(regex_pattern, page_data.decode("utf-8"))
        pdf_text += ''.join(m)
        print(''.join(m))

# write the text to a txt file
file = open(f'{file_path}/AWS_Well-Architected_Framework.txt','w')
file.write(pdf_text)
file.close()

print(f'TEXT FILE WRITTEN TO: {file_path}/{output_filename}')
