# Markdown elements sample (Word template tuning)

Use this file with `md-to-docx.ps1`, open the resulting DOCX in Word, and adjust **Styles** (Normal, Heading 1–6, List Paragraph, Quote, Intense Quote, Caption, Table styles, etc.). Save that document as your Pandoc **reference DOCX** if you replace the AI Policy template.

---

## Second-level heading

### Third-level heading

#### Fourth-level heading

##### Fifth-level heading

###### Sixth-level heading

---

## Body text and emphasis

This is a normal paragraph with **bold**, *italic*, ***bold italic***, and `inline code` in the same sentence.

Strikethrough (GitHub-flavored): ~~obsolete wording~~.

A second paragraph with a [link to Pandoc documentation](https://pandoc.org/MANUAL.html) and automatic line wrapping so you can see spacing between paragraphs in Word.

---

## Lists

### Unordered

- First item
- Second item
  - Nested item A
  - Nested item B
- Third item

### Ordered

1. Step one
2. Step two
   1. Sub-step 2a
   2. Sub-step 2b
3. Step three

### Task list (GFM)

- [x] Completed task
- [ ] Open task

---

## Blockquote

> This is a blockquote. Use it to tune **Quote** / **Intense Quote** in Word.

---

## Code

### Fenced block

```powershell
# Example only — tune "Source Code" / monospace font in the reference DOCX
Set-Location $PSScriptRoot
.\md-to-docx.ps1 ".\Type 2\Markdown-Elements-Style-Sample.md"
```

```text
Plain preformatted block without a language tag.
```

---

## Table

| Column A | Column B | Column C |
|----------|----------|----------|
| Row 1    | Data     | 123      |
| Row 2    | More     | 456      |
| **Bold** | *Italic* | `code`   |

---

## Horizontal rule

Above and below this section is a horizontal rule (`---`).

---

## Footnote

Pandoc can emit footnotes from Markdown.[^sample]

[^sample]: This footnote text helps you style footnote references and separators in Word.

---

## Afterword

Regenerate the DOCX after you change this file: from the `Documentation` folder run:

`.\md-to-docx.ps1 ".\Type 2\Markdown-Elements-Style-Sample.md"`

The output path will be `Type 2\Word Docs\Markdown-Elements-Style-Sample.docx`.
