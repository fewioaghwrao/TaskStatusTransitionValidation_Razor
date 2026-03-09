(() => {
    const loader = document.getElementById("pageLoader");
    const csvDialog = document.getElementById("csvExportDialog");
    const openCsvButton = document.getElementById("openCsvExportDialogButton");
    const csvForm = document.getElementById("csvExportForm");
    const csvSubmitButton = document.getElementById("csvExportSubmitButton");
    const csvFileBaseNameInput = document.getElementById("csvFileBaseName");

    function showLoader() {
        if (!loader) return;
        loader.classList.remove("is-hidden");
    }

    function hideLoader() {
        if (!loader) return;
        loader.classList.add("is-hidden");
    }

    function openCsvDialog() {
        if (!csvDialog) return;

        csvDialog.classList.add("is-open");
        csvDialog.setAttribute("aria-hidden", "false");

        if (csvFileBaseNameInput) {
            csvFileBaseNameInput.focus();
            csvFileBaseNameInput.select();
        }
    }

    function closeCsvDialog() {
        if (!csvDialog) return;

        csvDialog.classList.remove("is-open");
        csvDialog.setAttribute("aria-hidden", "true");

        if (csvSubmitButton) {
            csvSubmitButton.disabled = false;
            csvSubmitButton.textContent = "ダウンロード";
        }
    }

    window.addEventListener("load", () => {
        hideLoader();
    });

    document.querySelectorAll(".js-show-loader").forEach((el) => {
        el.addEventListener("click", () => {
            if (el.classList.contains("is-disabled")) return;
            if (el.hasAttribute("disabled")) return;
            showLoader();
        });
    });

    document.querySelectorAll(".js-show-loader-form").forEach((form) => {
        form.addEventListener("submit", () => {
            showLoader();
        });
    });

    if (openCsvButton) {
        openCsvButton.addEventListener("click", () => {
            openCsvDialog();
        });
    }

    if (csvDialog) {
        csvDialog.querySelectorAll("[data-csv-close]").forEach((el) => {
            el.addEventListener("click", () => {
                closeCsvDialog();
            });
        });

        csvDialog.addEventListener("click", (e) => {
            if (e.target === csvDialog) {
                closeCsvDialog();
            }
        });
    }

    document.addEventListener("keydown", (e) => {
        if (e.key === "Escape" && csvDialog?.classList.contains("is-open")) {
            closeCsvDialog();
        }
    });

    if (csvForm) {
        csvForm.addEventListener("submit", () => {
            if (csvSubmitButton) {
                csvSubmitButton.disabled = true;
                csvSubmitButton.textContent = "ダウンロード中…";
            }

            // CSVダウンロードは画面遷移ではないため全面ローダーは出さない
            // 少し待ってからモーダルだけ閉じる
            window.setTimeout(() => {
                closeCsvDialog();
            }, 800);
        });
    }
})();