using System;
using System.Threading;
using System.Windows.Controls;
using FluentAssertions;
using NEdifis.Attributes;
using NUnit.Framework;

namespace NZazu.Fields
{
    [TestFixtureFor(typeof (NZazuTextField))]
    [Apartment(ApartmentState.STA)]
    // ReSharper disable InconsistentNaming
    internal class NZazuTextField_Should
    {
        [Test]
        public void Be_Creatable()
        {
            var sut = new NZazuTextField("test");

            sut.Should().NotBeNull();
            sut.Should().BeAssignableTo<INZazuWpfField>();

            sut.Type.Should().Be("string");
        }

        [Test]
        [STAThread]
        public void Create_TextBox_with_ToolTip_Matching_Description()
        {
            var sut = new NZazuTextField("test")
            {
                Hint = "superhero",
                Description = "check this if you are a registered superhero"
            };

            var textBox = (TextBox)sut.ValueControl;
            textBox.Should().NotBeNull();
            textBox.Text.Should().BeEmpty();
            textBox.ToolTip.Should().Be(sut.Description);
        }

        [Test]
        [STAThread]
        public void Create_ValueControl_Even_If_Empty_Hint()
        {
            var sut = new NZazuTextField("test");

            var textBox = (TextBox)sut.ValueControl;
            textBox.Should().NotBeNull();
            textBox.Text.Should().BeEmpty();
        }

        [Test]
        [STAThread]
        public void Get_Set_Value_should_propagate_to_ValueControl_Without_LostFocus()
        {
            var sut = new NZazuTextField("test");
            var textBox = (TextBox)sut.ValueControl;
            textBox.Should().NotBeNull();

            sut.StringValue.Should().BeNull();
            textBox.Text.Should().BeNullOrEmpty();

            sut.StringValue = "test";
            sut.StringValue.Should().Be("test");


            textBox.Text.Should().Be("test");

            textBox.Text = string.Empty;
            sut.StringValue.Should().BeEmpty();
        }
    }
}