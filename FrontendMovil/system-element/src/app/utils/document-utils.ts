export class DocumentUtils {
  public static downloadFile(href: Blob | string, documentName: string) {
    const linkElement = document.createElement('a');
    linkElement.download = documentName;
    linkElement.href =
      typeof href === 'string' ? href : URL.createObjectURL(href);
    document.body.appendChild(linkElement);
    linkElement.click();
    document.body.removeChild(linkElement);
  }
}
