window.downloadFileFromStream = async (fileName, contentStreamReference) => {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName ?? '';
    anchorElement.click();
    anchorElement.remove();
    URL.revokeObjectURL(url);
}

window.downloadFileFromBase64 = async (fileName, base64) => {
    const anchorElement = document.createElement('a');
    anchorElement.href = `data:application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;${base64}`;
    anchorElement.download = fileName ?? '';
    anchorElement.click();
    anchorElement.remove();
}

window.clickOnElement = (elementId) => {
    document.getElementById(elementId).click();
}

window.StepWizardScrollToTop = () => {
    document.getElementById("scroll_anchor").scrollIntoView({ behavior: 'smooth' });
    //document.documentElement.scrollTop = 0;
}

window.FixChartGoalMarkerClip = () => {
    const elements = document.querySelectorAll('[id^="gridRectMarkerMask"]');
    elements.forEach(el => el.remove());
}

window.HideScrollBars = () => {
    const elements = [...document.querySelectorAll('*')];

    elements.filter(el => el.className !== "rz-form-field-content")
    .forEach(el =>
    {
        el.style.overflow = 'hidden';
    });

    elements.filter(el => el.tagName.toLowerCase() !== "i")
    .forEach(el => {
        el.style.fontFamily = "Roboto, sans-serif";
    });

    elements.forEach(el => {
        el.style.PrintColorAdjust = "exact";
    });

    [...document.getElementsByClassName("rz-body")].forEach(el => {
        el.style.backgroundColor = "rgba(255,255,255,0.90)";
        el.style.backgroundBlendMode = "lighten";
    });

    document.getElementById("dashboard-row-1").style.height = "43vh"
    document.getElementById("dashboard-row-2").style.height = "43vh"
}


window.downloadPdfFromBase64 = (base64Data, fileName) => {
    const link = document.createElement('a');
    link.href = `data:application/pdf;base64,${base64Data}`;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};

window.focusRadzenInput = (elementId) => {
    const element = document.getElementById(elementId)?.firstElementChild;
    element?.focus();
}