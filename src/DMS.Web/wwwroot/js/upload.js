(function () {
    'use strict';

    function getCsrfToken() {
        return document.querySelector('meta[name="csrf-token"]')?.content ?? '';
    }

    function setStatus(msg, type) {
        const el = document.getElementById('upload-status');
        if (!el) return;
        el.className = 'alert alert-' + (type ?? 'info');
        el.textContent = msg;
        el.classList.remove('d-none');
    }

    function clearStatus() {
        const el = document.getElementById('upload-status');
        if (el) el.classList.add('d-none');
    }

    function setProgress(pct) {
        const wrap = document.getElementById('upload-progress-wrap');
        const bar  = document.getElementById('upload-progress');
        if (!wrap || !bar) return;
        if (pct === 0) {
            wrap.classList.add('d-none');
        } else {
            wrap.classList.remove('d-none');
            bar.style.width = pct + '%';
            bar.setAttribute('aria-valuenow', pct);
        }
    }

    async function postForm(url, data) {
        const body = new URLSearchParams(data);
        const res = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'X-CSRF-TOKEN': getCsrfToken(),
            },
            body: body.toString(),
        });
        return res;
    }

    const form = document.getElementById('dms-upload-form');
    if (!form) return;

    form.addEventListener('submit', async function (e) {
        e.preventDefault();
        clearStatus();

        const fileInput = document.getElementById('file-input');
        const notesInput = document.getElementById('notes-input');
        const documentTypeInput = document.getElementById('document-type-input');
        const file = fileInput?.files?.[0];

        if (!file) {
            setStatus('Molimo odaberite datoteku prije slanja.', 'warning');
            return;
        }

        const MAX_BYTES = 3 * 1024 * 1024;
        if (file.size > MAX_BYTES) {
            setStatus('Datoteka prelazi ograničenje od 3 MB. Molimo odaberite manju datoteku.', 'warning');
            return;
        }

        const documentType = documentTypeInput?.value ?? '';
        if (!documentType) {
            setStatus('Molimo odaberite vrstu dokumenta.', 'warning');
            return;
        }

        const submitBtn = form.querySelector('[type="submit"]');
        if (submitBtn) submitBtn.disabled = true;

        try {
            setStatus('Generišem link za otpremanje...', 'info');
            setProgress(15);

            const urlRes = await postForm('/Dms?handler=RequestUpload', {
                fileName:    file.name,
                contentType: file.type || 'application/octet-stream',
                documentType: documentType,
                sizeBytes:   file.size,
                notes:       notesInput?.value ?? '',
            });

            const urlData = await urlRes.json();

            if (!urlRes.ok) {
                setStatus('Greška: ' + (urlData.error ?? urlRes.statusText), 'danger');
                return;
            }

            const { documentId, uploadUrl } = urlData;

            setStatus('Otpremam u sigurnu pohranu...', 'info');
            setProgress(40);

            const r2Res = await fetch(uploadUrl, {
                method: 'PUT',
                headers: { 'Content-Type': file.type || 'application/octet-stream' },
                body: file,
            });

            if (!r2Res.ok) {
                setStatus('Otpremanje u pohranu nije uspjelo (HTTP ' + r2Res.status + '). Pokušajte ponovo.', 'danger');
                return;
            }

            setProgress(75);
            setStatus('Potvrđujem otpremanje...', 'info');

            const confirmRes = await postForm('/Dms?handler=Confirm', { documentId });
            const confirmData = await confirmRes.json();

            if (!confirmRes.ok) {
                setStatus('Potvrda nije uspjela: ' + (confirmData.error ?? confirmRes.statusText), 'danger');
                return;
            }

            setProgress(100);
            setStatus('Otpremanje je završeno. Dokument je spreman.', 'success');
            form.reset();

            htmx.trigger(document.body, 'refresh-docs');

            setTimeout(() => {
                const panel = document.getElementById('upload-panel');
                if (panel) bootstrap.Collapse.getOrCreateInstance(panel).hide();
                clearStatus();
            }, 2500);
        } catch (err) {
            setStatus('Došlo je do neočekivane greške: ' + err.message, 'danger');
        } finally {
            setProgress(0);
            if (submitBtn) submitBtn.disabled = false;
        }
    });
})();
