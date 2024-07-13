var VRS;
(function (VRS) {
    var WebAdmin;
    (function (WebAdmin) {
        var SqlServerPluginOptions;
        (function (SqlServerPluginOptions) {
            var PageHandler = (function () {
                function PageHandler(viewId) {
                    this._ViewId = new WebAdmin.ViewId('SqlServerPluginOptions', viewId);
                    this.refreshState();
                }
                PageHandler.prototype.showFailureMessage = function (message) {
                    var alert = $('#failure-message');
                    if (message && message.length) {
                        alert.text(message || '').show();
                    }
                    else {
                        alert.hide();
                    }
                };
                PageHandler.prototype.refreshState = function () {
                    var _this = this;
                    this.showFailureMessage(null);
                    this._ViewId.ajax('GetState', {
                        success: function (data) {
                            _this.applyState(data);
                        },
                        error: function () {
                            setTimeout(function () { return _this.refreshState(); }, 5000);
                        }
                    }, false);
                };
                PageHandler.prototype.save = function () {
                    var _this = this;
                    this._Model.SaveAttempted(false);
                    var ajaxSettings = this.buildAjaxSettingsForSendConfiguration();
                    ajaxSettings.success = function (data) {
                        if (data.Exception) {
                            _this.showFailureMessage(VRS.stringUtility.format(VRS.WebAdmin.$$.WA_Exception_Reported, data.Exception));
                            _this._Model.SaveSuccessful(false);
                        }
                        else {
                            if (data.Response && data.Response.Outcome) {
                                _this._Model.SaveAttempted(true);
                                _this._Model.SaveSuccessful(data.Response.Outcome === "Saved");
                                switch (data.Response.Outcome || "") {
                                    case "Saved":
                                        _this._Model.SavedMessage(VRS.WebAdmin.$$.WA_Saved);
                                        break;
                                    case "FailedValidation":
                                        _this._Model.SavedMessage(VRS.WebAdmin.$$.WA_Validation_Failed);
                                        break;
                                    case "ConflictingUpdate":
                                        _this._Model.SavedMessage(VRS.WebAdmin.$$.WA_Conflicting_Update);
                                        break;
                                }
                            }
                            ko.viewmodel.updateFromModel(_this._Model, data.Response.ViewModel);
                        }
                    };
                    this._ViewId.ajax('Save', ajaxSettings);
                };
                PageHandler.prototype.testConnection = function () {
                    var _this = this;
                    this._Model.TestConnectionOutcomeMessage('');
                    this._Model.TestConnectionOutcomeTitle('');
                    var ajaxSettings = this.buildAjaxSettingsForSendConfiguration();
                    ajaxSettings.success = function (outcome) {
                        if (outcome.Exception) {
                            _this.showFailureMessage(VRS.stringUtility.format(VRS.WebAdmin.$$.WA_Exception_Reported, outcome.Exception));
                        }
                        else {
                            _this.showFailureMessage(null);
                            _this._Model.TestConnectionOutcomeMessage(outcome.Response.Message);
                            _this._Model.TestConnectionOutcomeTitle(outcome.Response.Title);
                            ko.viewmodel.updateFromModel(_this._Model, outcome.Response.ViewModel);
                            $('#test-connection-outcome').modal('show');
                        }
                    };
                    this._ViewId.ajax('TestConnection', ajaxSettings);
                };
                PageHandler.prototype.updateSchema = function () {
                    var _this = this;
                    this._Model.UpdateSchemaOutcomeMessage('');
                    this._Model.UpdateSchemaOutcomeTitle('');
                    var ajaxSettings = this.buildAjaxSettingsForSendConfiguration();
                    ajaxSettings.success = function (outcome) {
                        if (outcome.Exception) {
                            _this.showFailureMessage(VRS.stringUtility.format(VRS.WebAdmin.$$.WA_Exception_Reported, outcome.Exception));
                        }
                        else {
                            _this.showFailureMessage(null);
                            var joinedOutputLines = outcome.Response.OutputLines.join('\n');
                            _this._Model.UpdateSchemaOutcomeMessage(joinedOutputLines);
                            _this._Model.UpdateSchemaOutcomeTitle(outcome.Response.Title);
                            ko.viewmodel.updateFromModel(_this._Model, outcome.Response.ViewModel);
                            $('#update-schema-outcome').modal('show');
                        }
                    };
                    this._ViewId.ajax('UpdateSchema', ajaxSettings);
                };
                PageHandler.prototype.buildAjaxSettingsForSendConfiguration = function () {
                    var _this = this;
                    var viewModel = ko.viewmodel.toModel(this._Model);
                    var result = {
                        method: 'POST',
                        data: {
                            viewModel: JSON.stringify(viewModel)
                        },
                        dataType: 'json',
                        error: function (jqXHR, textStatus, errorThrown) {
                            _this.showFailureMessage(VRS.stringUtility.format(VRS.WebAdmin.$$.WA_Send_Failed, errorThrown));
                        }
                    };
                    return result;
                };
                PageHandler.prototype.applyState = function (state) {
                    if (state.Exception) {
                        this.showFailureMessage(VRS.stringUtility.format(VRS.WebAdmin.$$.WA_Exception_Reported, state.Exception));
                    }
                    else {
                        this.showFailureMessage(null);
                        if (this._Model) {
                            ko.viewmodel.updateFromModel(this._Model, state.Response);
                        }
                        else {
                            this._Model = ko.viewmodel.fromModel(state.Response, {
                                arrayChildId: {},
                                extend: {
                                    '{root}': function (root) {
                                        root.SaveAttempted = ko.observable(false);
                                        root.SaveSuccessful = ko.observable(false);
                                        root.SavedMessage = ko.observable('');
                                        root.TestConnectionOutcomeTitle = ko.observable('');
                                        root.TestConnectionOutcomeMessage = ko.observable('');
                                        root.UpdateSchemaOutcomeTitle = ko.observable('');
                                        root.UpdateSchemaOutcomeMessage = ko.observable('');
                                    }
                                }
                            });
                            ko.applyBindings(this._Model);
                        }
                    }
                };
                return PageHandler;
            }());
            SqlServerPluginOptions.PageHandler = PageHandler;
        })(SqlServerPluginOptions = WebAdmin.SqlServerPluginOptions || (WebAdmin.SqlServerPluginOptions = {}));
    })(WebAdmin = VRS.WebAdmin || (VRS.WebAdmin = {}));
})(VRS || (VRS = {}));
//# sourceMappingURL=SqlServerPluginOptions.js.map