module Stacker.Web.Views.Partials.Forms

open System
open Stacker.Calculate
open Stacker.Domain
open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx
open Stacker.Charting.Domain
open Stacker.GenerateSeries
open Stacker.Web.Models

let importForm (errs: string list) =
    div [] [
        div [
            _class "modal modal-backdrop fade show"
            _style "display:block;"
        ] []
        div
            [ _class "modal show modal-blur"
              _style "display:block;"
              _tabindex "-1" ] [
                div [ _class "modal-dialog" ] [
                    div [ _class "modal-content"; ] [
                        div [ _class "modal-header" ] [
                            h1 [ _class "modal-title" ] [ str "Transaction Details" ]
                            button [ _type "button"; _class "btn-close"; _onclick "closeModal()" ] []
                        ]
                        div [ _class "modal-body" ] [
                            div [] (errs |> List.map (fun err -> p [] [str err]))
                            div [ _class "row mb-3" ] [
                                div [] [
                                    str "CSV file should be in the following format. Hopefully more formats are coming."
                                ]
                                pre [] [
                                    str "Type,Buy,BuyCurrency,Sell,SellCurrency,Date\nTrade,0.05224,BTC,2000,CAD,2023-04-03 21:51"
                                ]
                            ]
                            
                            form [
                                _enctype "multipart/form-data"
                                _hxPost "/import"
                                _hxTarget "this"
                                _hxSwap "outerHTML"
                            ] [
                                div [ _class "row mb-3" ] [
                                    div [ _class "col-sm-12 col-md-6" ] [
                                        label [ _class "form-label"; _required; ] [ str "CSV File" ]
                                        input [ _type "file"; _name "CsvFile"; _required; _class "form-control" ]
                                        input [ _type "hidden"; _name "Other"; _value "Testing" ]
                                    ]
                                ]
                                div [ _class "row" ] [
                                    div
                                        [ _class "col" ]
                                        [ button
                                              [ _class "btn btn-secondary"
                                                _onclick "closeModal()" ]
                                              [ str "Cancel" ] ]
                                    div
                                        [ _class "col-auto" ]
                                        [ button
                                              [ _type "submit"
                                                _class "btn btn-success" ]
                                              [ str "Import" ]
                                            ]
                                    ]
                            ]
                        ]
                        ]
                    ]
                ]
            ]
    
let newTxsForm (errs: string list) =
    form
        [ _hxPost "/bought"
          _hxTarget "this"
          _hxSwap "outerHTML" ]
        [
            div [ _class "row" ] (errs |> List.map (fun e -> div [ _class "col-sm-12 text-red" ] [ str e ]))
            
            div [ _class "mb-3" ] [
                div [ _class "col-sm-12" ] [
                    label [ _class "form-label" ] [ str "Date and Time" ]
                    input [ _type "hidden"; _name "timeZoneOffset"; _id "time-zone-offset"; _value "" ]
                    input [
                        _type "datetime-local"
                        _name "date"
                        _required
                        _class "form-control"
                        _value (DateTime.Now.ToString("yyyy-MM-ddTHH:mm"))
                        ]
                ]
            ]            
            div
                [ _class "row mb-3" ]
                [
                    div [ _class "col-sm-12" ] [
                        label [ _class "form-label"; _required;  ] [ str "Transaction Type" ]
                        select [ _name "Type"; _required; _class "form-select" ] [
                            option [ _value "Buy"; _selected ] [ str "Buy" ]
                            option [ _value "Sell"; ] [ str "Sell" ]
                            option [ _value "Income"; ] [ str "Income" ]
                            option [ _value "Spend"; ] [ str "Spend" ]
                        ]
                        div [ _class "form-text" ] [ str "The kind of transaction" ]
                    ]
                ]
            div [ _class "row mb-3" ] [
                div [ _class "col-sm-12" ] [
                    label [ _class "form-label"; _required ] [ str "Bitcoin Units" ]
                    select [ _name "btcUnit"; _required; _class "form-select" ] [
                          option [ _value "Btc"; _selected ] [ str "BTC" ]
                          option [ _value "Sats"; ] [ str "SATS" ]
                    ]
                    div [ _class "form-text" ] [ str "Btc or Sats" ]
                ]
            ]
            div [ _class "row mb-3" ] [
                div [ _class "col-sm-12" ] [
                    label [ _class "form-label"; _required; _min "0" ] [ str "BTC Amount" ]
                    input [ _name "btcAmount"; _required; _class "form-control" ]
                    div [ _class "form-text" ] [ str "Amount of BTC involved in the transaction" ]
                ]
            ]
            hr []
            div [ _class "row mb-3" ] [
                div [ _class "col-sm-12" ] [
                    label [ _class "form-label"; _required; _min "0" ] [ str "Fiat Amount" ]
                    input [ _name "fiatAmount"; _required; _class "form-control" ]
                    div [ _class "form-text" ] [ str "Amount of Fiat involved in the transaction" ]
                ]
            ]
            div [ _class "row mb-3" ] [
                div [ _class "col-sm-12" ] [
                    label [ _class "form-label"; _required; _min "0" ] [ str "Fiat Currency" ]
                    select [ _type "text"; _name "fiat"; _required; _class "form-select" ] [
                        option [ _value "CAD" ] [ str "CAD" ]
                        option [ _value "USD" ] [ str "USD" ]
                    ]
                    div [ _class "form-text" ] [ str "Fiat currency involved in the transaction" ]
                ]
            ]

            div [ _class "row" ] [
                div [ _class "col" ] [
                    button  [ _class "btn btn-secondary"; _onclick "closeModal()" ]  [ str "Cancel" ]
                ]
                div [ _class "col-auto" ] [
                    button [ _type "submit"; _class "btn btn-success" ] [ str "Save" ]
                ]
            ]
        ]
    
let newTxModal =
    div [] [
        div [
            _class "modal modal-backdrop fade show"
            _style "display:block;"
        ] []
        div
            [ _class "modal show modal-blur"
              _style "display:block;"
              _tabindex "-1" ] [
                div [ _class "modal-dialog" ] [
                    div
                        [ _class "modal-content"; ]
                        [
                            div [ _class "modal-header" ] [
                                h1 [ _class "modal-title" ] [ str "New Transaction" ]
                                button [ _type "button"; _class "btn-close"; _onclick "closeModal()" ] []
                            ]
                            div [ _class "modal-body" ] [
                                newTxsForm []
                                script [] [
                                    rawText
                                        """
                                        var timezoneOffset = new Date().getTimezoneOffset();
                                        document.getElementById("time-zone-offset").value = timezoneOffset; 
                                        """
                                    ]                
                            ] ] ]
                   ]
    ]

let newNoteForm (newNote: NewNoteForm) =
   form [
        _hxPost "/note"
        _hxTarget "this"
        _hxSwap "outerHTML"
    ] [
        div [ _class "row mb-3" ] [
            div [ _class "col-sm-12 col-md-4" ] [
                label [ _class "form-label"; _for "sentiment" ] [ str "Sentiment" ]
                select [ _class "form-control form-select"; _name "sentiment" ]
                    (Sentiment.AllCases |> List.map (fun name -> option [ _value name ] [ str name ]))
            ]
            div [ _class "col-sm-12 col-md-4" ] [
                label [ _class "form-label"; _for "noteDate" ] [ str "Date" ]
                input [ _type "text"; _name "noteDate"; _disabled; _class "form-control form-input"; _value (newNote.CurrentDate.ToString("yyyy-MM-dd")) ] 
            ]
            div [ _class "col-sm-12 col-md-4" ] [
                label [ _class "form-label"; _for "currentPrice" ] [ str "Current Price" ]
                input [ _name "currentPrice"; _disabled; _class "form-control form-input"; _value $"""{newNote.CurrentPrice} - {newNote.Fiat}""" ] 
            ]
        ]
        div [ _class "row mb-3" ] [
            div [ _class "col-sm-12" ] [
                label [ _class "form-label"; _for "noteBody" ] [ str "What's on your mind?" ]
                textarea [
                    _style "min-height: 300 px"
                    _class "form-control"
                    _name "noteBody"
                    _id "note-body"
                ] []
            ]
        ]
        
        div [ _class "row" ] [
            div [ _class "col" ] [
                button [
                    _class "btn btn-secondary"; _onclick "closeModal()"
                ] [ str "Cancel" ]
            ]
            div [ _class "col-auto" ] [
                button [
                    _type "submit"
                    _class "btn btn-success"
                ]  [ str "Save" ]
            ]
        ]
    ]

let newNoteModal (newNote: NewNoteForm) =
    div [] [
        div [
            _class "modal modal-backdrop fade show"
            _style "display:block;"
        ] []
        div [
            _class "modal show modal-blur"
            _style "display:block;"
            _tabindex "-1" ] [
                div [ _class "modal-dialog modal-lg" ] [
                    div [
                        _class "modal-content";
                    ] [
                        div [ _class "modal-header" ] [
                            h1 [ _class "modal-title" ] [ str "Take a Note" ]
                            button [ _type "button"; _class "btn-close"; _onclick "closeModal()" ] []
                        ]
                        div [ _class "modal-body" ] [
                            newNoteForm newNote
                        ]
                    ]
                ]
        ]
        script [ _src "/js/rich-text-editor.js" ] []
    ]

let noteDetailsForm (note: Note) =
    form [] [
        div [ _class "row mb-3" ] [
            div [ _class "col-sm-12 col-md-4" ] [
                label [ _class "form-label"; _for "sentiment" ] [ str "Sentiment" ]
                select [ _class "form-control form-select"; _name "sentiment"; _disabled ] [
                    if note.Sentiment.IsSome then
                        let sentiment = note.Sentiment.Value.ToString()
                        option [ _value sentiment; _selected ] [ str sentiment ]
                    else
                        option [] []
                ]
            ]
            div [ _class "col-sm-12 col-md-4" ] [
                label [ _class "form-label"; _for "noteDate" ] [ str "Date" ]
                input [ _type "text"; _name "noteDate"; _disabled; _class "form-control form-input"; _value (note.Date.ToString("yyyy-MM-dd")) ] 
            ]
            div [ _class "col-sm-12 col-md-4" ] [
                label [ _class "form-label"; _for "currentPrice" ] [ str "Price" ]
                input [ _name "currentPrice"; _disabled; _class "form-control form-input"; _value $"""{note.BtcPrice} - {note.Fiat}""" ] 
            ]
        ]
        div [ _class "row mb-3" ] [
            div [ _class "col-sm-12" ] [
                label [ _class "form-label"; _for "noteBody" ] [ str "" ]
                textarea [
                    _class "form-control"
                    _name "noteBody"
                    _rows "12"
                    _id "note-body"
                ] [ str note.Text ]
            ]
        ]
    ]

let noteDetailsModal (note: Note) =
    div [] [
        div [
            _class "modal modal-backdrop fade show"
            _style "display:block;"
        ] []
        div [
            _class "modal show modal-blur"
            _style "display:block;"
            _tabindex "-1"
        ] [
            div [ _class "modal-dialog modal-lg" ] [
                div [ _class "modal-content"; ] [
                    div [ _class "modal-header" ] [
                        h1 [ _class "modal-title" ] [ note.Date.ToString("yyyy-MM-dd") |> str ]
                        button [ _type "button"; _class "btn-close"; _onclick "closeModal()" ] []
                    ]
                    div [ _class "modal-body" ] [
                        noteDetailsForm note
                    ]
                ]
            ]
        ]
        script [ _src "/js/rich-text-editor.js" ] []
    ]
    
let deleteModal (t: Transaction) =
    div [] [
        div [
            _class "modal modal-backdrop fade show"
            _style "display:block;"
        ] []
        div
            [ _class "modal show modal-blur"
              _style "display:block;"
              _tabindex "-1" ] [
                div [ _class "modal-dialog" ] [
                    div [ _class "modal-content"; ] [
                        div [ _class "modal-header" ] [
                            h1 [ _class "modal-title" ] [ str "Delete Transaction" ]
                            button [ _type "button"; _class "btn-close"; _onclick "closeModal()" ] []
                        ]
                        div [ _class "modal-body" ] [
                            form [
                                _hxPost "/delete"
                                _hxTarget "this"
                                _hxSwap "outerHtml"
                            ] [
                                div [ _class "row mb-3" ] [
                                    div [ _class "col-sm-12" ] [
                                        p [] [ str "Are you sure you want to delete this Transaction?" ]
                                    ]
                                ]
                                div [ _class "row mb-3" ] [
                                    div [ _class "col col-sm-4" ] [
                                        div [ _class "datagrid-item" ] [
                                            div [ _class "datagrid-title" ] [
                                                str "Tx Type"
                                            ]
                                            div [ _class "datagrid-content" ] [
                                                str $"{t.TxName}"
                                            ]
                                        ]    
                                    ]
                                    
                                    div [ _class "col col-sm-4" ] [
                                        div [ _class "datagrid-item" ] [
                                            div [ _class "datagrid-title" ] [
                                                str "Tx Date"
                                            ]
                                            div [ _class "datagrid-content" ] [
                                                str $"{t.DateTime.ToShortDateString()}"
                                            ]
                                        ]    
                                    ]
                                    div [ _class "col col-sm-4" ] [
                                        div [ _class "datagrid-item" ] [
                                            div [ _class "datagrid-title" ] [
                                                str "Tx Amount"
                                            ]
                                            div [ _class "datagrid-content" ] [
                                                let formatted = t.Amount |> decimal |> fun d -> d.ToString("F8")
                                                str formatted
                                            ]
                                        ]
                                    ]
                                ]
                                
                                div [ _class "row" ] [
                                    div [ _class "col" ] [
                                        button [ _class "btn btn-secondary"; _onclick "closeModal()" ] [ str "Cancel" ]
                                    ]
                                    div [ _class "col-auto" ] [
                                        button [
                                            _type "submit"
                                            _class "btn btn-danger"
                                            _hxDelete $"/tx/delete/{t.Id}"
                                        ] [
                                            str "Delete"
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
    ]
    
let editTxForm (t: Transaction) (errs: string list) =
    let btcAmount = (decimal t.Amount).ToString("F8")
    
    let fiatAmount =
        match t.Fiat with
        | None -> ""
        | Some f -> f.Amount.ToString()
        
    let fiatCurrency =
        match t.Fiat with
        | None -> ""
        | Some f -> string f.Currency    
    
    form [
        _hxPut $"/tx/edit/{t.Id}"
        _hxTarget "this"
        _hxSwap "outerHTML"
    ] [
        div [ _class "row" ] (errs |> List.map (fun e -> div [ _class "col-sm-12 text-red" ] [ str e ]))
        div [ _class "row mb-3" ] [
            div [ _class "col-sm-12 col-md-4" ] [
                label [ _class "form-label"; _required;  ] [ str "Tx" ]
                select [ _type "button"; _name "Type"; _required; _class "form-select" ] [
                    option
                        ([ _value "Buy" ] @ if t.TxName = "BUY" then [ _selected ] else [])
                        [ str "Buy" ]
                    option
                        ([ _value "Sell" ] @ if t.TxName = "SELL" then [ _selected ] else [] )
                        [ str "Sell" ]
                    option
                        ([ _value "Income" ] @ if t.TxName = "Income" then [ _selected ] else [])
                        [ str "Income" ]
                    option
                        ([ _value "Spend"; ] @ if t.TxName = "Spend" then [ _selected ] else [])
                        [ str "Spend" ]
                ]
            ]

            div [ _class "col-sm-12 col-md-6" ] [
                label [ _class "form-label"; _required; _min "0" ] [ str "Btc Amount" ]
                div [ _class "input-group" ] [
                    input [
                        _name "amount"
                        _required
                        _class "form-control"
                        _value btcAmount
                    ]
                    select [ _name "btcUnit"; _required; _class "form-select" ] [
                        option [ _value "Btc"; _selected ] [ str "BTC" ]
                        option [ _value "Sats"; ] [ str "SATS" ]
                    ]
                ]
            ]
        ]
        div [ _class "row mb-3" ] [
            div [ _class "col-sm-12 col-md-4" ] [
                label [ _class "form-label"; _required; _min "0" ] [ str "Fiat Amount" ]
                input [
                    _name "fiatAmount"
                    _required
                    _class "form-control"
                    _value fiatAmount
                ] ]
            div [ _class "col-sm-12 col-md-4" ] [
                label [ _class "form-label"; _required; _min "0" ] [ str "Fiat Currency" ]
                select [
                    _type "text"
                    _name "fiat"
                    _required
                    _class "form-select"
                ] [
                    option
                        ([ _value "CAD" ] @ if fiatCurrency = "CAD" then [ _selected ] else [])
                        [ str "CAD" ]
                    option
                        ([ _value "USD" ] @ if fiatCurrency = "USD" then [ _selected ] else [])
                        [ str "USD" ]
                ]
            ]
        ]

        div [ _class "mb-3" ] [
            label [ _class "form-label" ] [ str "Date" ]
            input [ _type "hidden"; _name "timeZoneOffset"; _id "time-zone-offset"; _value "" ]
            input [
                _type "datetime-local"
                _name "date"
                _required
                _class "form-control"
                _value (t.DateTime.ToString("yyyy-MM-ddTHH:mm"))
            ]
        ]

        div [ _class "row" ] [
            div [ _class "col" ] [
                button [ _class "btn btn-secondary"; _onclick "closeModal()" ] [
                    str "Cancel"
                ]
            ]
            div [ _class "col-auto" ] [
                button [
                    _type "submit"
                    _class "btn btn-success"
                ] [
                    str "Save"
                ]
            ]
        ]
    ]

let editModal (t: Transaction) =    
    div [] [
        div [
            _class "modal modal-backdrop fade show"
            _style "display:block;"
        ] []
        div
            [ _class "modal show modal-blur"
              _style "display:block;"
              _tabindex "-1" ] [
                div [ _class "modal-dialog" ] [
                    div [ _class "modal-content"; ] [
                        div [ _class "modal-header" ] [
                            h1 [ _class "modal-title" ] [ str "Edit Transaction" ]
                            button [ _type "button"; _class "btn-close"; _onclick "closeModal()" ] []
                        ]
                        div [ _class "modal-body" ] [
                            editTxForm t []
                            script [] [
                                rawText
                                    """
                                    var timezoneOffset = new Date().getTimezoneOffset();
                                    document.getElementById("time-zone-offset").value = timezoneOffset; 
                                    """                                
                            ]
                        ]
                    ]
                ]
            ]
    ]

let txDetails (t: Transaction) (change: Change option) =
    div [] [
        div [
            _class "modal modal-backdrop fade show"
            _style "display:block;"
        ] []
        div
            [ _class "modal show modal-blur"
              _style "display:block;"
              _tabindex "-1" ] [
                div [ _class "modal-dialog" ] [
                    div [ _class "modal-content"; ] [
                        div [ _class "modal-header" ] [
                            h1 [ _class "modal-title" ] [ str "Transaction Details" ]
                            button [ _type "button"; _class "btn-close"; _onclick "closeModal()" ] []
                        ]
                        div [ _class "modal-body" ] [
                            div [ _class "row mb-3" ] [
                                div [ _class "col col-sm-4" ] [
                                    div [ _class "datagrid-item" ] [
                                        div [ _class "datagrid-title" ] [
                                            str "Tx Type"
                                        ]
                                        div [ _class "datagrid-content" ] [
                                            str $"{t.TxName}"
                                        ]
                                    ]    
                                ]
                                
                                div [ _class "col col-sm-4" ] [
                                    div [ _class "datagrid-item" ] [
                                        div [ _class "datagrid-title" ] [
                                            str "Tx Date"
                                        ]
                                        div [ _class "datagrid-content" ] [
                                            str $"{t.DateTime.ToShortDateString()}"
                                        ]
                                    ]    
                                ]
                                div [ _class "col col-sm-4" ] [
                                    div [ _class "datagrid-item" ] [
                                        div [ _class "datagrid-title" ] [
                                            str "Tx Amount"
                                        ]
                                        div [ _class "datagrid-content" ] [
                                            let formatted = t.Amount |> decimal |> fun d -> d.ToString("F8")
                                            i [ _class "ti ti-coin-bitcoin text-yellow"; _alt "BTC" ] []
                                            str formatted
                                        ]
                                    ]
                                ]
                            ]
                            div [ _class "row mb-3" ] [
                                div [ _class "col col-sm-4" ] [
                                    div [ _class "datagrid-item" ] [
                                        div [ _class "datagrid-title" ] [
                                            str "Fiat"
                                        ]
                                        div [ _class "datagrid-content" ] [
                                            let fiat =
                                                match t.Fiat with
                                                | Some f ->
                                                    let asStr = f.Amount.ToString("C2")
                                                    $"{asStr} - {f.Currency}"
                                                | None -> ""
                                            
                                            str fiat
                                        ]
                                    ]    
                                ]
                                
                                div [ _class "col col-sm-4" ] [
                                    div [ _class "datagrid-item" ] [
                                        div [ _class "datagrid-title" ] [
                                            str "Fiat Price Per Coin"
                                        ]
                                        div [ _class "datagrid-content" ] [
                                            match t.PricePerCoin with
                                            | None -> div [] []
                                            | Some (amt, fiat) ->
                                                let amtStr = amt.ToString("C2")
                                                str $"{amtStr} - {fiat}"
                                        ]
                                    ]    
                                ]
                                
                                div [ _class "col col-sm-4" ] [
                                    div [ _class "datagrid-item" ] [
                                        div [ _class "datagrid-title" ] [
                                            str "% Change"
                                        ]
                                        div [ _class "datagrid-content" ] [
                                            match change with
                                            | None -> div [] []
                                            | Some (Increase amt) ->
                                                div [ _style "color: green;" ] [ str $"{amt}%%" ]
                                            | Some (Decrease percent) ->
                                                div [] [ str $"{percent}%%" ]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
    ]
    
    
let userSettings (settings: UserSettingsVm) =
    let fiatOption (fiatOption: string) (preferredFiat: Fiat) =
        let attrs = [ _value fiatOption; ] @ (if preferredFiat.ToString() = fiatOption then [ _selected ] else []) 
        option attrs [ str fiatOption ]
        
    let tzs =
        TimeZoneInfo.GetSystemTimeZones()
        |> Seq.map (fun tz -> (tz.Id, tz.DisplayName))
        |> Seq.toList
    
    form [
        _hxPost "/settings/user-settings"
        _hxSwap "outerHTML"
        _hxTarget "this"
        ] [
        div [ _class "row mb-3" ] [
            div [ _class "col" ] [
                span [ _class "avatar avatar-xl"; _style $"background-image: url(https://robohash.org/{settings.UserName}.png)" ] [ ]
                div [ _class "form-text" ] [ str "This unique image is generated from your username" ]
            ]
        ]
        div [ _class "row mb-3" ] [
            div [ _class "col-sm-12 col-md-4" ] [
                label [ _class "form-label required"; _required;  ] [ str "Username" ]
                input [ _class "form-control"; _disabled; _value settings.UserName ]
            ]
        ]
        
        div [ _class "row mb-3" ] [
            div [ _class "col-sm-12 col-md-3" ] [
                label [ _class "form-label required"; _required;  ] [ str "Preferred Fiat" ]
                select [ _type "button"; _name "Fiat"; _required; _class "form-select" ] [
                    fiatOption "USD" settings.PreferredFiat
                    fiatOption "CAD" settings.PreferredFiat
                    fiatOption "EUR" settings.PreferredFiat
                ]
            ]
        ]
        div [ _class "row mb-3" ] [
            div [ _class "col-sm-12 col-md-4" ] [
                label [ _class "form-label required"; _required;  ] [ str "Timezone" ]
                select [ _type "button"; _name "Timezone"; _required; _class "form-select" ]
                    (tzs |> List.map (fun (id, name) -> option [ _value id ] [ str name ] ))
            ]
        ]
        div [ _class "row mb-3" ] [
            div [ _class "loading-spinner"; _class "col" ] [
                div [ _class "spinner-border text-blue"; ] []
            ]
            div [ _class "col-auto" ] [
                button [ _type "submit"; _class "btn btn-primary" ] [ str "Save" ]
            ]
        ]
    ]

let workbenchFormulaDesigner (formulaValue: string option) (graphableSeries: (string * GraphableDataSeries) list) =
    div [ _id "formula-designer" ] [
        let v = Option.defaultValue "" formulaValue
        div [ _class "row mb-3" ] [
            div [ _class "btn-list" ] [
                button [ _class "btn" ] [
                    str "BTC Price  "
                    span [ _class "badge bg-yellow ms-2" ] [ str "btc-usd" ]
                ]
            ]
        ]
        
        div [] (graphableSeries |> List.map (fun (name, g) ->
            div [] [
                span [] [ str $"{name} {(g.ToString())}" ]
            ]))
        form [ _id "workbench-formula-form"; _hxPost "/workbench/formula"; _hxTarget "#formula-designer"; _hxSwap "outerHTML" ] [
            div [ _class "row mb-3" ] [
                div [ _class "col col-md-3" ] [
                    label [ _class "form-label" ] [ str "formula name" ]
                    input [ _class "form-control"; _name "formulaName"; _placeholder "200 wma"; _required ]
                ]
                div [ _class "col col-md-3" ] [
                    label [ _class "form-label" ] [ str "formula" ]
                    input [ _class "form-control"; _name "formula"; _required; _placeholder "sma(btc-usd, 700)" ]
                ]
            ]
            button [ _type "submit"; _class "btn btn-primary" ] [ str "Save and Draw" ]
            
        ]
    ]
    
let dcaCalculatorForm (generateDca: GenerateRequest) =
    let date = generateDca.Start.Date.ToShortDateString()
    let cadenceOptionDropdown (name: string) =
        option ([ _value name; ] @ if name = string generateDca.Cadence then [ _selected ] else []) [ str name ]
        
    let durationOptionDropdown (name: string) =
        let _, durationUnit = generateDca.Duration
        option ([ _value name; ] @ if name = string durationUnit then [ _selected ] else []) [ str name ]        
        
    form [
        _hxPost "/dca-calculator"
        _id "dca-calculator-form"
        _hxTarget "#dca-calculator-form"
        _hxSwap "outerHTML"
    ] [
        div [ _class "row mb-3" ] [
            div [ _class "col-sm-6 col-md-2" ] [
                label [ _class "form-label" ] [ str "Start Date" ]
                input [ _name "StartDate"; _type "date"; _class "form-control"; _value date ]
            ]
            div [ _class "col-sm-6 col-md-2" ] [
                label [ _class "form-label" ] [ str "DCA Amount" ]
                input [ _name "Amount"; _type "number"; _class "form-control"; _value $"{generateDca.FiatAmount}" ]
            ]
            div [ _class "col-sm-6 col-md-2" ] [
                label [ _class "form-label" ] [ str "Cadence" ]
                select [ _name "Cadence"; _class "form-control form-select" ] [
                    cadenceOptionDropdown "Daily"
                    cadenceOptionDropdown "Weekly"
                    cadenceOptionDropdown "Monthly"
                ]
            ]
            div [ _class "col-sm-6 col-md-3" ] [
                label [ _class "form-label" ] [ str "Duration" ]
                div [ _class "row" ] [
                    div [ _class "col" ] [
                        input [ _name "Duration"; _type "number"; _class "form-control"; _value $"{generateDca.Duration |> fst}" ]
                        
                    ]
                    div [ _class "col" ] [
                        select [ _name "DurationUnit"; _class "form-control form-select" ] [
                            durationOptionDropdown "Days"
                            durationOptionDropdown "Weeks"
                            durationOptionDropdown "Months"
                            durationOptionDropdown "Years"
                        ]
                    ]
            
                    

                ]
            ]
        ]
        div [ _class "row" ] [
            div [ _class "col-sm-3" ] [
                button [ _class "btn btn-primary"; _type "submit" ] [ str "Calculate" ]    
            ]
        ]
    ]    