

// ---------- Global helper functions ----------
function getBalance() {
    const acc = $("#AccountNumber").val();
    if (!acc) {
        $("#BalanceBefore,#NewBalance,#AccountHolderName,#Description").val("");
        return;
    }

    // Balance
    $.get("/AccountTransaction/GetBalance", { accNo: acc }, d => {
        $("#BalanceBefore").val(d.balance);
        calcNewBalance();
    });

    // Account‑holder name
    $.get("/AccountTransaction/GetAccountHolder", { accNo: acc }, d => {
        $("#AccountHolderName").val(d.accountHolder);
    });
}

function calcNewBalance() {
    const before = parseFloat($("#BalanceBefore").val()) || 0;
    const amount = parseFloat($("#TransAmount").val()) || 0;
    const type = $("#TransType").val();
    let result = type === "CR" ? before + amount : before - amount;

    if (type === "DR" && result < 0) {
        alert("❌ Debit not allowed. Resulting balance would be negative.");
        result = before;
    }
    $("#NewBalance").val(result.toFixed(2));
}

function clearForm() {
    $("#transForm")[0].reset();
    $("#AccountHolderName,#BalanceBefore,#NewBalance,#TransId").val("");
}

// ---------- DOM ready ----------
$(function () {

    // ----- SAVE -----
    $('#transForm').on('submit', function (e) {
        e.preventDefault();
        if (!$(this).valid()) return;

        $.post('/AccountTransaction/SaveTransaction',
            $(this).serialize(),
            res => {
                alert(res.success ? "✅ " + res.message : "❌ " + res.message);
                if (res.success) clearForm();
            }).fail(() => alert("❌ Server error on save."));
    });

    // ----- LOAD (search) -----
    $('#btnSearch').on('click', loadTransaction);
    function loadTransaction() {
        const transId = $("#TransId").val();
        if (!transId) { alert("❌ Enter a Transaction ID."); return; }

        $.get("/AccountTransaction/GetTransactionById", { transId }, d => {
            if (!d) { alert("❌ Transaction not found."); return; }

            $("#AccountNumber").val(d.accountNumber);
            $("#AccountHolderName").val(d.accountHolderName);
            $("#BalanceBefore").val(d.balanceBefore);
            $("#TransType").val(d.transType);
            $("#Description").val(d.description);
            $("#TransAmount").val(d.transAmount);
            $("#NewBalance").val(d.newBalance);
        }).fail(() => alert("❌ Failed to load transaction."));
    }

    // ----- UPDATE -----
    $('#btnUpdate').on('click', function () {
        const token = $('input[name="__RequestVerificationToken"]').val();
        const data = {
            __RequestVerificationToken: token,
            TransId: $("#TransId").val(),
            AccountNumber: $("#AccountNumber").val(),
            AccountHolderName: $("#AccountHolderName").val(),
            BalanceBefore: $("#BalanceBefore").val(),
            Description: $("#Description").val(),
            TransType: $("#TransType").val(),
            TransAmount: $("#TransAmount").val(),
            NewBalance: $("#NewBalance").val()
        };

        $.post("/AccountTransaction/UpdateTransaction", data, res => {
            alert(res.success ? "✅ Transaction updated!" : "❌ " + res.message);
            if (res.success) clearForm();
        }).fail(xhr => alert("❌ Server error on update:\n" + xhr.responseText));
    });

    // ----- DELETE -----
    $('#btnDelete').on('click', deleteTransaction);
    function deleteTransaction() {
        const transId = $("#TransId").val();
        if (!transId) { alert("❌ Enter a Transaction ID to delete."); return; }
        if (!confirm("Delete this transaction?")) return;

        const token = $('input[name="__RequestVerificationToken"]').val();
        $.post("/AccountTransaction/DeleteTransaction",
            { transId, __RequestVerificationToken: token },
            res => {
                alert(res.success ? "✅ Transaction deleted!" : "❌ " + res.message);
                if (res.success) clearForm();
            }).fail(xhr => alert("❌ Server error on delete:\n" + xhr.responseText));
    }
});
